# ICMS Database Schema v1

**Stack:** PostgreSQL 18 · .NET 10 · EF Core
**Scale target:** 100k+ member rows per district

## Design Rules

| Rule | Why |
|---|---|
| `BIGINT` identity PKs | Headroom for multi-district (10M members) future |
| `DECIMAL(18,2)` for all money | Financial correctness (M1 lesson: never `double`) |
| `DateTimeOffset` UTC | Ethiopia UTC+3; offset survives timezone changes |
| Soft delete everywhere (`is_deleted`) | "Data is never deleted" business rule |
| `church_id` on every church-scoped table (`NULL` = district level) | Hierarchical scoping: district / local / daughter / member |
| EFGBC ID as business key | `EFGBC-<DISTRICT-CODE>-<6 digits>`, unique, never changes |
| Append-only history | Transfers/clearance are immutable rows |
| Enums as `INT` | Simple, fast |
| pg_trgm GIN index on member names | Fast name search at 100k rows |

## Entity Groups

1. [Identity & Access](#1-identity--access)
2. [Church Hierarchy](#2-church-hierarchy)
3. [Members](#3-members)
4. [Transfers (immutable)](#4-transfers-immutable)
5. [Teams](#5-teams)
6. [Finance](#6-finance)
7. [HR / Employment](#7-hr--employment)
8. [Departments](#8-departments)
9. [Daughter Church Upgrade](#9-daughter-church-upgrade)
10. [Public Website](#10-public-website)

---

## 1. Identity & Access

> JWT/auth deferred to a later phase. `User` table is baseline foundation only.

### User
```
id                 BIGINT PK
username           VARCHAR(50) UNIQUE
password_hash      TEXT
role               INT        -- DistrictAdmin | ChurchAdmin | Member | Employee | HQ
church_id          BIGINT NULL FK -> Church
member_id          BIGINT NULL FK -> Member
is_active          BOOL
created_at         TIMESTAMPTZ
```
- Staff who aren't members: `member_id = NULL`.
- Member who is also staff: both set. One login per person.

---

## 2. Church Hierarchy

### Church — local AND daughter in one table (upgrade = flag flip)
```
id                 BIGINT PK
district_id        BIGINT FK -> District
name               VARCHAR(200)
type               INT        -- Local | Daughter
parent_church_id   BIGINT NULL FK -> Church   -- set only for daughters
code               VARCHAR(10)
city               VARCHAR(100)
subcity            VARCHAR(100)
email              VARCHAR(150) NULL
phone              VARCHAR(20)
tel                VARCHAR(20) NULL          -- landline
map_address        TEXT                      -- Google Maps link / coordinates
website_url        VARCHAR(255) NULL
is_deleted         BOOL
created_at         TIMESTAMPTZ
```
INDEX `IX_Church_DistrictId`, `IX_Church_ParentChurchId`

### ServiceTime — weekly program (separate table, not a JSON blob)
```
id                 BIGINT PK
church_id          BIGINT FK -> Church
day_of_week        INT        -- 0 = Sunday ... 6 = Saturday
start_time         TIME
end_time           TIME NULL
service_type       VARCHAR(50)  -- "Sunday Worship", "Wednesday Prayer"
```
INDEX `IX_ServiceTime_ChurchId_DayOfWeek`

### District — one row now, schema ready for 42
```
id                 BIGINT PK
name               VARCHAR(200)
code               VARCHAR(10)
address            VARCHAR(255)
```

---

## 3. Members

### Member — the 100k-row table
```
id                 BIGINT PK
efgbc_id           VARCHAR(30) UNIQUE NOT NULL   -- EFGBC-NWAA-000001
church_id          BIGINT NOT NULL FK -> Church  -- current church
first_name         VARCHAR(100)                  -- given name
father_name        VARCHAR(100)                  -- father's name
grandfather_name   VARCHAR(100)                  -- Ethiopian "last name"
date_of_birth      DATE
gender             INT
job_status         INT      -- Student|Employed|SelfEmployed|Unemployed|Retired|Other
phone              VARCHAR(20)
email              VARCHAR(150) NULL
photo_url          VARCHAR(255) NULL             -- for Digital ID
status             INT      -- Active|Transferring|Deactivated|Archived
deactivation_reason INT NULL -- NULL|ClearanceOut|Deceased|Penalised|Other
joined_via         INT      -- Baptism|Salvation|Transfer|Return
joined_at          DATE
is_deleted         BOOL
created_at         TIMESTAMPTZ
```
INDEXES
- `UX_Member_EfgbcId` UNIQUE
- `IX_Member_ChurchId` + `IX_Member_ChurchId_Status`
- `IX_Member_Name` — pg_trgm GIN (first/father/grandfather) for name search
- `IX_Member_Phone`, `IX_Member_Email`

Rule: `deactivation_reason` required when `status = Deactivated`. Deceased flows through `DeathRecord` (source of truth) which sets `Archived`.

### MemberFamily — head-of-household + links
```
id                 BIGINT PK
member_id          BIGINT FK -> Member
family_name        VARCHAR(200)
head_of_household_member_id BIGINT FK -> Member
```
### FamilyMember
```
id                 BIGINT PK
family_id          BIGINT FK -> MemberFamily
member_id          BIGINT FK -> Member
relation           INT       -- Self|Spouse|Child|Parent|Sibling|Other
```
### MemberDocument
```
id                 BIGINT PK
member_id          BIGINT FK -> Member
type               INT       -- Photo|BaptismCert|Education|Other
file_url           VARCHAR(255)
uploaded_at        TIMESTAMPTZ
```
### BaptismRecord
```
id                 BIGINT PK
member_id          BIGINT FK -> Member
baptism_date       DATE
church_id          BIGINT FK -> Church
officiated_by      VARCHAR(100) NULL
```
### LearningRecord
```
id                 BIGINT PK
member_id          BIGINT FK -> Member
title              VARCHAR(150)
year               INT
status             INT
```
### DeathRecord
```
id                 BIGINT PK
member_id          BIGINT FK -> Member (unique)
death_date         DATE
church_id          BIGINT FK -> Church
recorded_by        VARCHAR(100) NULL
```
### PrayerRequest
```
id                 BIGINT PK
member_id          BIGINT FK -> Member
content            TEXT
status             INT
submitted_at       TIMESTAMPTZ
responded_by       BIGINT NULL FK -> User
response           TEXT NULL
responded_at       TIMESTAMPTZ NULL
```

---

## 4. Transfers (immutable)

### Transfer — one immutable row per transfer attempt
```
id                 BIGINT PK
member_id          BIGINT FK -> Member
from_church_id     BIGINT FK -> Church
to_church_id       BIGINT NULL FK -> Church   -- NULL for Exit
destination_name   VARCHAR(200) NULL          -- Exit: "left EFGBC", "moved to Bahir Dar"
type               INT       -- Internal|Exit|Return
status             INT       -- Initiated|ClearanceIssued|Accepted|Rejected|Deactivated|Reactivated|Voided
initiated_by       INT       -- Member | Admin
initiated_by_user_id BIGINT NULL FK -> User
void_reason        VARCHAR(255) NULL
voided_by_user_id  BIGINT NULL FK -> User
voided_at          TIMESTAMPTZ NULL
closed_at          TIMESTAMPTZ NULL
is_deleted         BOOL
created_at         TIMESTAMPTZ
```
INDEXES `IX_Transfer_MemberId`, `IX_Transfer_FromChurchId_Status`

Rules
- Append-only. Rejection/void = closed record, never deleted.
- One pending transfer per member (status in Initiated|ClearanceIssued).
- `Member.status` drives state: Transferring during pending internal transfer.

### ClearanceCertificate — immutable snapshot, 1:1 with a transfer
```
id                 BIGINT PK
transfer_id        BIGINT FK -> Transfer (unique)
certificate_code   VARCHAR(30) UNIQUE
member_id          BIGINT FK -> Member
from_church_id     BIGINT FK -> Church
to_church_id       BIGINT NULL FK -> Church
issued_at          TIMESTAMPTZ
issued_by_user_id  BIGINT NULL FK -> User
```

---

## 5. Teams

### Team — self-referencing tree for sub-teams
```
id                 BIGINT PK
church_id          BIGINT FK -> Church
parent_team_id     BIGINT NULL FK -> Team   -- Choir -> Choir A / Choir B
team_type          INT    -- Choir|Worship|BiblePreach|Prayer|Development|Media|Youth|Women|Other
name               VARCHAR(150)             -- "Choir A", "Tuesday"
is_deleted         BOOL
created_at         TIMESTAMPTZ
```
UNIQUE INDEXES (Postgres: plain UNIQUE treats NULLs as distinct)
```
UNIQUE (church_id, name)                  WHERE parent_team_id IS NULL
UNIQUE (church_id, parent_team_id, name)  WHERE parent_team_id IS NOT NULL
```
- Differentiates "Choir A of Bole" from "Choir A of Megenagna" (church_id scope).
- Blocks duplicate "Choir A" under the same parent in the same church.

### TeamMember
```
id                 BIGINT PK
team_id            BIGINT FK -> Team
member_id          BIGINT FK -> Member
role               INT       -- Member | Leader
joined_at          TIMESTAMPTZ
is_deleted         BOOL
```
UNIQUE `(team_id, member_id)` WHERE is_deleted = FALSE
### TeamAttendance — large table (teams × members × weeks)
```
id                 BIGINT PK
team_id            BIGINT FK -> Team
member_id          BIGINT FK -> Member
attendance_date    DATE
attended           BOOL
```
INDEX `IX_TeamAttendance_TeamId_Date`
### TeamPayment — monthly social payments per member
```
id                 BIGINT PK
team_id            BIGINT FK -> Team
member_id          BIGINT FK -> Member
month              DATE
amount             DECIMAL(18,2)
paid_at            TIMESTAMPTZ
```
INDEX `IX_TeamPayment_TeamId_Month`, `IX_TeamPayment_MemberId`
### TeamActivity
```
id                 BIGINT PK
team_id            BIGINT FK -> Team
title              VARCHAR(150)
activity_date      DATE
description        TEXT NULL
notes              TEXT NULL
```

---

## 6. Finance

### Budget — annual/quarterly plans
```
id                 BIGINT PK
church_id          BIGINT NULL FK -> Church   -- NULL = district budget
year               INT
quarter            INT NULL
amount             DECIMAL(18,2)
is_deleted         BOOL
```
### Expense
```
id                 BIGINT PK
church_id          BIGINT NULL FK -> Church   -- NULL = district expense
category           VARCHAR(100)
amount             DECIMAL(18,2)
description        TEXT NULL
expense_date       DATE
approved_by        BIGINT NULL FK -> User
is_deleted         BOOL
```
INDEX `IX_Expense_ChurchId_Date`
### Tithe — per-member tithes & offerings
```
id                 BIGINT PK
church_id          BIGINT FK -> Church
member_id          BIGINT NULL FK -> Member   -- NULL = anonymous basket
type               INT       -- Tithe | Offering
amount             DECIMAL(18,2)
income_date        DATE
recorded_by        BIGINT NULL FK -> User
```
INDEX `IX_Tithe_ChurchId_Date`, `IX_Tithe_MemberId`

---

## 7. HR / Employment

### Employee — one row per position, person can hold multiple
```
id                 BIGINT PK
church_id          BIGINT NULL FK -> Church   -- NULL = district office
user_id            BIGINT NULL FK -> User
member_id          BIGINT NULL FK -> Member
employment_type    INT    -- FulltimeMinister|DistrictStaff|ChurchStaff|Volunteer
minister_title     INT NULL  -- Pastor|Evangelist|Prophet|Teacher|Apostle (only for FulltimeMinister)
position           VARCHAR(100)   -- "District President", "Head of Development Dept", "Cashier"
is_district_president BOOL DEFAULT FALSE
hire_date          DATE
salary             DECIMAL(18,2) NULL
status             INT
is_deleted         BOOL
```
UNIQUE INDEXES (one active position per person per scope)
```
UNIQUE (member_id, church_id) WHERE is_deleted = FALSE
UNIQUE (user_id,   church_id) WHERE is_deleted = FALSE
```
- Pastor Samuel example:
  - row 1: `church_id=Bole`, FulltimeMinister, title=Pastor → Bole employment list
  - row 2: `church_id=NULL`, DistrictStaff, position="District President", is_district_president=true → district staff list
  - Same person in Bole member list AND Bole employment list AND district staff list.
- Minister count = `COUNT(*) WHERE employment_type = FulltimeMinister AND is_deleted = FALSE`.

---

## 8. Departments

### Department — district (`church_id NULL`) and church level
```
id                 BIGINT PK
church_id          BIGINT NULL FK -> Church
name               VARCHAR(150)
type               INT    -- Spiritual|Charity|Development
head_employee_id   BIGINT NULL FK -> Employee   -- points at the person's district/church row
```
### DepartmentMember
```
id                 BIGINT PK
department_id      BIGINT FK -> Department
member_id          BIGINT FK -> Member
role               INT
```
### DepartmentActivity
```
id                 BIGINT PK
department_id      BIGINT FK -> Department
title              VARCHAR(150)
activity_date      DATE
description        TEXT NULL
```

---

## 9. Daughter Church Upgrade

### UpgradeApplication
```
id                 BIGINT PK
daughter_church_id BIGINT FK -> Church
status             INT    -- Submitted|InReview|Approved|Rejected|Conditional
submitted_at       TIMESTAMPTZ
decided_by         BIGINT NULL FK -> User
decision_at        TIMESTAMPTZ NULL
feedback           TEXT NULL
conditions         TEXT NULL
condition_deadline DATE NULL
```
INDEX `IX_UpgradeApplication_DaughterChurchId`
- Approved → `Church.type = Local`, `parent_church_id = NULL`. No data migration.

---

## 10. Public Website (district page only)

### BlogPost
```
id                 BIGINT PK
author_user_id     BIGINT FK -> User
title              VARCHAR(200)
content            TEXT
published_at       TIMESTAMPTZ NULL
is_published       BOOL
is_deleted         BOOL
```
### ContactMessage
```
id                 BIGINT PK
name               VARCHAR(100)
email              VARCHAR(150)
phone              VARCHAR(20) NULL
message            TEXT
submitted_at       TIMESTAMPTZ
handled            BOOL
```

---

## Reports

No report tables. District/church reports (member counts, growth, finance summaries, daughter roll-ups) are query-time aggregations using the indexes above. Materialized views only if a query proves slow at scale.

## SQL Migration Order (FK dependencies)

1. District
2. Church, ServiceTime
3. User
4. Member, MemberFamily, MemberDocument, BaptismRecord, LearningRecord, DeathRecord, PrayerRequest
5. Transfer, ClearanceCertificate
6. Team, TeamMember, TeamAttendance, TeamPayment, TeamActivity
7. Budget, Expense, Tithe
8. Employee
9. Department, DepartmentMember, DepartmentActivity
10. UpgradeApplication
11. BlogPost, ContactMessage
