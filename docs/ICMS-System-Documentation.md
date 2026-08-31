# EFGBC Integrated Church Management System (ICMS)

**Project:** EFGBC Integrated Church Management System
**Developer:** Samuel Yonas (Qiyas-2026-006431)
**Date:** June 2026
**Scope:** North West Addis Ababa & Surrounding Region (Ketena)

---

## 1. What is ICMS?

ICMS is a digital platform that manages church operations for the Ethiopian Full Gospel Believers Church (EFGBC). It replaces manual paper records with a digital system that connects the district office, local churches, and members.

### Current Situation
- Member records are managed manually on paper
- Regional offices receive only total attendance counts
- No visibility into member movement or activity
- Daughter church upgrades tracked informally
- No self-service portal for members

### What ICMS Does
- Digitizes member records for the entire district
- Gives the district office real-time visibility into all churches
- Creates a unique digital identity for every member
- Streamlines member transfers between churches
- Automates daughter church upgrade workflow
- Provides self-service portal for members

---

## 2. Who Uses ICMS?

### District Office (Ketena)
Manages all churches in the district, oversees departments, monitors finances, and reports to HQ. This is the main administrator.

### Local Churches (100+)
Manage their members, finances, departments, and daughter churches. Each church has its own dashboard and public website.

### Daughter Churches (9,000+)
Small churches with limited features. They report to a local church and can apply for upgrade to become local churches.

### Members (100,000+)
Individual members who can access their profile, digital ID card, submit prayer requests, and request transfers.

### HQ (Read-Only)
Headquarters has view-only access to reports and statistics across all districts.

---

## 3. How ICMS Works (By Module)

### 3.1 Member Management

**Registration:**
Members are registered by their church when they join through baptism, salvation, or transfer. Each member receives a unique EFGBC ID that stays with them for life.

**Digital ID Card:**
Every member gets a digital ID card with their photo, EFGBC ID, church information, and a QR code for verification.

**Member Portal:**
Members can login to:
- View and update their profile
- Access their digital ID card
- Submit prayer requests
- Request transfers
- Upload documents

**Member Status:**
- Active: Current member in good standing
- Transferring: In the process of moving to another church
- Deactivated: Left EFGBC (data preserved)
- Archived: Deceased or permanently removed

---

### 3.2 Transfer System

The transfer system works like a **banking system** where members "move" between churches.

#### Types of Transfers:

**1. Internal Transfer (Within District)**
- Member moves from Church A to Church B in the same district
- Requires approval from both churches
- Clearance certificate is generated
- Full history is recorded

**How it works:**
1. Member requests transfer
2. Church A initiates the transfer in the system
3. System generates a clearance certificate
4. Church B receives the request
5. Church B reviews and decides to accept or reject
6. If accepted: Member moves to Church B
7. If rejected: Member stays in Church A

**2. Exit Transfer (Outside District or EFGBC)**
- Member leaves the district or denomination
- No approval needed (auto-approved)
- Destination name is recorded
- Member is deactivated but data is preserved

**How it works:**
1. Member leaves EFGBC or moves to another district
2. Church Admin enters the destination name
3. System generates clearance
4. Member is deactivated
5. Data is preserved for future reactivation

**3. Return Transfer**
- Former member rejoins EFGBC
- Reactivated with same EFGBC ID
- All history preserved
- Assigned to new church

**How it works:**
1. Member returns to EFGBC
2. Church Admin searches by name or EFGBC ID
3. System finds the deactivated member
4. Member is reactivated
5. Assigned to current church
6. Return is recorded in history

---

### 3.3 Daughter Church Upgrade

Daughter churches are small congregations that want to become fully operational local churches.

**Upgrade Process:**

**Phase 1: Application**
- Daughter church applies for upgrade
- Submits required documents
- Shows growth metrics (members, finances, etc.)

**Phase 2: Review**
- District reviews the application
- Checks: Member count, financial stability, leadership, facilities

**Phase 3: Decision**
- Approved: Daughter becomes local church
- Rejected: Feedback provided for improvement
- Conditional: Meet conditions within 6 months

**Phase 4: Implementation**
- Church type changes from Daughter to Local
- Full features are unlocked
- Assigned to a local church for oversight
- Members are notified

**What Changes After Upgrade:**
| Feature | Before (Daughter) | After (Local) |
|---------|-------------------|---------------|
| Member Management | Read-only | Full control |
| Finance Management | Submit only | Full control |
| Department Management | Not available | Available |
| Employee Management | Not available | Available |
| Dashboard | Basic | Full dashboard |

---

### 3.4 Department Management

Departments organize church activities and ministries.

**District Departments:**
- Youth Ministry
- Bible Ministry
- Theology
- Media
- Missionary
- Charity

**Church Departments:**
- Spiritual
- Charity
- Team Management

**Department Functions:**
- Assign leaders and members
- Track activities and events
- Generate reports
- Monitor participation

---

### 3.5 Finance Management

**District Level:**
- Budget management
- Expense tracking
- Financial reports

**Church Level:**
- Offerings and tithes
- Expense tracking
- Monthly financial reports

**Plans:**
- Annual financial plans
- Quarterly plans

**Reports:**
- Financial summaries
- Church contribution reports
- HQ reports (PDF)

---

### 3.6 Public Website

**District Website:**
- Home page with welcome message and latest news
- About page with leadership and vision
- List of all churches (searchable)
- Each church has a dynamic detail page
- Contact form

**Church Websites:**
- Each church has a dynamic page showing:
  - Church name and address
  - Service times
  - Pastor's name
  - Member count
  - Latest blog posts
- Churches can link to their own custom website

---

## 4. Business Rules

### 4.1 Member Rules
- Every member gets a unique EFGBC ID (permanent)
- EFGBC ID never changes
- Member belongs to exactly one church at a time
- Members are identified by EFGBC ID or Name + DOB

### 4.2 Transfer Rules
- Internal transfers require approval from both churches
- Exit transfers are auto-approved
- Clearance is required for all transfers
- Transfer history is preserved forever
- Data is never deleted (soft delete only)

### 4.3 Church Rules
- Daughter churches cannot manage members or finances
- Daughter churches must report to a local church
- Only local churches can have daughter churches
- Church upgrades require district approval

### 4.4 Data Rules
- All data is preserved (soft delete)
- Transfer history is immutable
- Only district can approve daughter church upgrades
- HQ has read-only access

---

## 5. Key Workflows

### 5.1 New Member Registration
Person joins church
↓
Church Admin registers in system
↓
System generates EFGBC ID
↓
Digital ID card is created
↓
Member account is created
↓
Welcome email is sent
↓
Member can login

### 5.2 Member Transfer (Internal)
Member requests transfer
↓
Church A initiates transfer
↓
System generates clearance
↓
Church B receives request
↓
Church B reviews
↓
Accept? → Member moves to Church B
Reject? → Member stays in Church A
↓
History updated

### 5.3 Daughter Church Upgrade
Daughter church applies
↓
District reviews application
↓
Check criteria (members, finances, etc.)
↓
Approve? → Upgrade to local church
Reject? → Feedback provided
↓
Features unlocked
↓
Members notified

---

## 6. User Access

| What They Can Do | District Admin | Church Admin | Member | HQ (Read-Only) |
|------------------|---------------|--------------|--------|----------------|
| Manage Churches | ✅ | ❌ | ❌ | ❌ |
| Manage Members | ✅ | ✅ | ❌ | ❌ |
| Approve Transfers | ✅ | ✅ | ❌ | ❌ |
| Manage Finances | ✅ | ✅ | ❌ | ❌ |
| View Reports | ✅ | ✅ | ❌ | ✅ |
| View Own Profile | ✅ | ✅ | ✅ | ✅ |
| Request Transfer | ❌ | ❌ | ✅ | ❌ |
| Submit Prayer | ❌ | ❌ | ✅ | ❌ |

---

## 7. Reports & Analytics

### District Reports
- Total members by church
- Member growth trends
- Financial summaries
- Department performance
- Church activity reports

### Church Reports
- Member demographics
- Financial reports
- Department reports
- Transfer statistics

### HQ Reports
- District-wide statistics
- Annual reports
- Growth analytics
- Financial summaries
- Printable PDFs

---

## 8. Summary

### What ICMS Provides:
- ✅ Digital member records
- ✅ Unique identity for every member
- ✅ Real-time visibility for district
- ✅ Streamlined transfers (banking-like)
- ✅ Automated daughter church upgrades
- ✅ Self-service member portal
- ✅ Department management
- ✅ Financial tracking and reporting
- ✅ Public church websites

### Key Features:
- One system for district, churches, and members
- Single database with multi-tenant design
- Role-based access for different users
- Digital ID cards with QR codes
- Transfer history preserved
- Real-time dashboards
- HQ read-only access
