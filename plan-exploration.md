# ICMS Project Exploration Summary

## 1. Overall Project Structure

**Root Directory:** `C:\Users\ijool\ICMS`

**Top-level directories and files:**
- `.claude/` - Claude configuration
- `AGENTS.md` - Agent guidelines for the EFGBC ICMS project
- `docs/` - Documentation (database schema, system documentation, UI mockups)
- `icms-web/` - **Angular frontend application**
- `Icms.Api/` - ASP.NET Core Web API backend
- `Icms.Application/` - Application layer (CQS, services)
- `Icms.Domain/` - Domain entities and business rules
- `Icms.Infrastructure/` - EF Core infrastructure and data access
- `ICMS.slnx` - Solution file

**Key subdirectories in `icms-web/`:**
- `src/` - Source code
  - `app/` - Application component and all features
  - `environments/` - Environment configs (dev/prod)
  - `stores/` - NGRX signal stores
  - `services/` - HTTP services
  - `models/` - TypeScript interfaces
  - `app.ts` - Root component
  - `app.config.ts` - Angular providers
  - `main.ts` - Bootstrap entry point
  - `tailwind.css` - Tailwind directives
  - `styles.scss` - Global SCSS with Angular Material theming
- `package.json` - Dependencies and scripts
- `angular.json` - Angular CLI configuration
- `tsconfig.json/app.json/spec.json` - TypeScript configs
- `proxy.conf.json` - API proxy config

## 2. Angular-related Files & Version

**Angular Version:** `v22.1.3`

**Evidence from `icms-web/package.json`:**
```json
"@angular/animations": "^22.1.2",
"@angular/cdk": "^22.1.2",
"@angular/common": "^22.1.0",
"@angular/compiler": "^22.1.0",
"@angular/core": "^22.1.0",
"@angular/forms": "^22.1.0",
"@angular/material": "^22.1.2",
"@angular/platform-browser": "^22.1.0",
"@angular/router": "^22.1.0",
"@ngrx/signals": "^22.0.0-rc.0",
"@angular/build": "^22.1.3",
"@angular/cli": "^22.1.3",
"@angular/compiler-cli": "^22.1.0",
```

**`angular.json`** confirms Angular CLI v22 with:
- `packageManager: "npm"`
- Builder: `@angular/build:application`
- Styles: `src/tailwind.css`, `src/styles.scss`

## 3. Current Angular Version

**v22.1.3** - This is a very recent Angular version (released 2025+). Key characteristics:
- Full signal-based reactivity (NGRX signals integration)
- `@angular/core` uses signals as the primary reactivity model
- `@ngrx/signals` ^22.0.0-rc.0 - NGRX integrated with signals
- Angular Material v22.1.2 fully signal-compatible
- Tailwind CSS v4 integration via `@tailwindcss/vite`

## 4. UI Component Library / Styling Approach

**Primary Styling Framework: Tailwind CSS v4**

**Evidence:**
```json
"tailwindcss": "^4.3.3",
"@tailwindcss/postcss": "^4.3.3",
"@tailwindcss/vite": "^4.3.3",
"postcss": "^8.5.26",
```

**Configuration:**
- `src/tailwind.css` - `@import "tailwindcss";` with custom theme variables
  ```css
  @theme {
    --color-brand: #f048a8;
    --color-brand-strong: #e0309b;
    --color-brand-blue: #1878c0;
    --color-brand-blue-strong: #11629e;
  }
  ```
- `src/styles.scss` - Angular Material theming using `mat.theme()` mixin
  - Brand colors overridden: `--mat-sys-primary: #f048a8`, `--mat-sys-tertiary: #1878c0`
  - Color scheme: `light` default, can be `dark`
  - Font and system variable configuration

**UI Component Library: Angular Material v22.1.2**

**Evidence from package.json and code:**
- `@angular/material: "^22.1.2"` used throughout
- Material components imported in components: `MatFormField`, `MatInput`, `MatButton`, `MatIcon`, `MatProgressSpinner`, `MatSnackBar`, `MatSelect`, `MatTable`, `MatPaginator`, `MatSort`, etc.
- Material theming configured in `styles.scss` via `@use '@angular/material' as mat;`
- `mat.theme()` mixin for Material 3 design spec

**Combined Approach:** Tailwind CSS v4 for utility-first styling + Angular Material v22 for complete UI components. The brand colors (pink #f048a8 and blue #1878c0 from the EFGBC logo) are defined in both Tailwind `@theme` block and Angular Material system variables.

## 5. Current State of the Frontend (icms-web)

**Application Type:** Single-page Angular application with client-side routing and lazy loading.

**Architecture Patterns:**
- **Signal-based reactivity** throughout (Angular 22 native signals + NGRX signalStore)
- **Feature modules** organized by domain (district, church, member, auth, landing)
- **Lazy loading** all route modules via `loadComponent`
- **SignalStore pattern** with NGRX for state management (district.store, church.store, member.store)
- **rxResource** for data fetching in components
- **Computed signals** for derived state

**Feature Structure:**
- `landing` - Home page with district data via rxResource
- `auth` - Login, unauthorized, area-select (area selection)
- `district` - District admin shell with navigation, dashboard, profile, members, churches, employees, departments, ministers, accounts, payments, settings, reports
- `member` - Member portal shell (limited to dashboard + areas switch)
- `church` - Church shell with profile, members, ministers, office, settings

**Key Technical Details:**
- **Bootstrap:** `bootstrapApplication(App, appConfig)` in `main.ts` (standalone)
- **Routing:** Angular v22 routes with `canActivate` guards (`authGuard`, `roleGuard`)
- **HTTP Interceptors:** `credentialsInterceptor` (withCredentials), `jwtInterceptor` (Authorization Bearer), `errorInterceptor` (401 refresh, 403 handling, MatSnackBar messages)
- **State Management:** NGRX signalStore with `withState`, `withComputed`, `withMethods`, `withEntities`, `rxMethod`
- **Forms:** Reactive Forms with `FormBuilder`, `Validators`, `ReactiveFormsModule`
- **HTTP:** `HttpClient` with `withFetch()`, `withInterceptors`, `withXsrfConfiguration`
- **Material:** Angular Material v22 with custom theming (primary: pink, tertiary: blue)
- **Routing Strategy:** `withComponentInputBinding` for route data injection

**Navigation Structure:**
- District admin: `/district` → dashboard, profile, members, churches, employees, departments, ministers, payments, accounts, settings, reports
- Church: `/church` → profile, members, ministers, office, settings  
- Member: `/member` → dashboard
- Auth: `/login`, `/unauthorized`, `/areas` (area select)

**Employee Salary Tracking:** Models include `SalaryPaidBy: 'Church' | 'District'` type, with computed stores tracking `paidFromDistrict` count from employee entities.
</summary>

Key directories and their purpose:
- `icms-web/src/app/features/landing/` - Home page
- `icms-web/src/app/features/auth/` - Authentication flows
- `icms-web/src/app/features/district/` - District admin features (core)
- `icms-web/src/app/features/church/` - Church features
- `icms-web/src/app/features/member/` - Member portal features
- `icms-web/src/app/features/shared/` - Shared components (module-placeholder)
- `icms-web/src/app/stores/` - NGRX signal stores
- `icms-web/src/app/services/` - HTTP services
- `icms-web/src/app/models/` - TypeScript interfaces
- `icms-web/src/app/guards/` - Route guards (auth, role)
- `icms-web/src/app/interceptors/` - HTTP interceptors