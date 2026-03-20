# Software Project Management Plan (SPMP)

## Group 7 Accounting System

**Course:** SWE4713 - Software Engineering | **Team:** Group 7 | **Version:** 1.0 | **Date:** [FILL IN]

---

## 1. Introduction

### 1.1 Purpose

This Software Project Management Plan (SPMP) defines the project management approach, team structure, schedule, risk strategy, and technical processes for the Group 7 Accounting System. It serves as the authoritative reference for how the project is organized, executed, monitored, and delivered. The document covers team roles and responsibilities, sprint planning, risk identification and mitigation, communication protocols, development environment specifications, coding standards, and a detailed work breakdown structure.

### 1.2 Project Overview

The Group 7 Accounting System is a web-based, multi-role accounting application built with ASP.NET Core 8 Razor Pages. The system serves three distinct user roles - Administrator, Manager, and Accountant - each with specific permissions and workflows. The application covers the full accounting cycle: chart of accounts management, journal entry creation and approval, ledger posting with running balances, financial report generation, and a financial ratio dashboard.

The system is designed to enforce proper accounting procedures (balanced debits and credits, approval workflows, audit trails) while providing a modern, browser-accessible interface. Built using Entity Framework Core for data access and ASP.NET Identity for authentication and authorization, the application demonstrates enterprise-level security patterns including password expiration, history-based reuse prevention, account lockout, and comprehensive event logging.

### 1.3 Definitions and Acronyms

| Term / Acronym | Definition |
|----------------|------------|
| SPMP | Software Project Management Plan |
| SRS | Software Requirements Specification |
| SDT | Software Design and Testing Document |
| EF Core | Entity Framework Core - Object-Relational Mapper for .NET |
| ASP.NET | Active Server Pages .NET - Microsoft's web application framework |
| COA | Chart of Accounts - the master list of all accounts in an organization |
| CRUD | Create, Read, Update, Delete - basic data operations |
| ORM | Object-Relational Mapper - translates between objects and database tables |
| IDE | Integrated Development Environment |
| PR | Post Reference - a link in the ledger back to the originating journal entry |
| IS | Income Statement - financial report showing revenue and expenses |
| BS | Balance Sheet - financial report showing assets, liabilities, and equity |
| RE | Retained Earnings - cumulative profits retained in the business |
| PBKDF2 | Password-Based Key Derivation Function 2 - password hashing algorithm |
| CSRF | Cross-Site Request Forgery - a web security attack vector |
| LINQ | Language Integrated Query - .NET query syntax used with EF Core |
| DTO | Data Transfer Object - object used to transfer data between layers |
| UTC | Coordinated Universal Time - standard time reference used in the database |
| SMTP | Simple Mail Transfer Protocol - standard for sending email |

---

## 2. Project Organization

### 2.1 Team Structure and Responsibilities

| Team Member | Role | Primary Responsibilities |
|-------------|------|--------------------------|
| [FILL IN] | Project Lead | Sprint planning, task assignment, GitHub management, integration oversight, instructor communication |
| [FILL IN] | Backend Developer | C# page models, EF Core data access, business logic, services (PostingService, EventLogger, AccountingMath) |
| [FILL IN] | Frontend Developer | Razor views (.cshtml), CSS styling (site.css), JavaScript (site.js), Bootstrap layout, form design |
| [FILL IN] | QA Lead | Manual testing, test case design, bug tracking, documentation coordination |

[FILL IN: Add or remove rows as needed for your actual team size and role assignments.]

### 2.2 External Interfaces

- **Course Instructor:** [FILL IN: Professor name] - receives all deliverables (Proposal, SPMP, SRS, SDT, User Manual, working software), evaluates project against sprint requirements, provides feedback and grading.
- **Version Control Host:** GitHub - Repository name: AD-AccountingSystem-Group7. All source code, migrations, and documentation are version-controlled here.
- **Deployment Host:** [FILL IN: Where is the application hosted for demonstration? Options include: local development server (localhost), Windows Server with IIS, Azure App Service, or university-provided hosting.]

---

## 3. Managerial Process

### 3.1 Project Lifecycle

The project follows an Agile/Scrum lifecycle with five sprints. Each sprint targets a specific functional module and produces a working, integrated software increment. At the end of each sprint, all new features are committed to the master branch after verifying successful compilation and basic functionality. The next sprint builds on the previous sprint's codebase, ensuring continuous integration throughout the project.

### 3.2 Sprint Plan

#### Sprint 1 - User Interface and Authentication Module

- **Goal:** Implement all user-facing authentication and user management features, establishing the security foundation for the entire application.
- **Features:**
  - Login page with username/email and hidden password field
  - Registration redirected to custom Access Request workflow
  - Three user roles created at startup: Administrator, Manager, Accountant
  - Default administrator account seeded (admin@local.test / Admin!234)
  - Password policy enforcement (min 8 chars, starts with letter, digit required, special char required)
  - Password expiration (90 days) with 3-day advance warning
  - Password history tracking (prevents reuse of last 5 passwords)
  - Account lockout after 3 failed login attempts (30-minute lockout)
  - Forgot password flow with security question verification
  - Password reset with token-based link
  - Administrator: create users via Access Request approval (auto-generate username)
  - Administrator: edit user roles, activate/deactivate, suspend with date range
  - Administrator: view all users report with search and role filtering
  - Administrator: view expired passwords report
  - Administrator: send email to any user (stored in database outbox)
  - Email outbox system (DbEmailSender stores all emails in SentEmails table)
  - Logged-in username displayed on all pages (via _LoginPartial)
- **Definition of Done:** All three user types can log in; administrator can create and manage users via Access Request workflow; password policies are enforced; lockout functions correctly; password reset via security questions works end-to-end.

#### Sprint 2 - Chart of Accounts Module

- **Goal:** Full chart of accounts management for administrators; view-only access for Manager and Accountant roles.
- **Features:**
  - Add new account with all required fields (name, number, description, normal side, category, subcategory, initial balance, order code, statement type IS/BS/RE, comment)
  - Edit existing account details
  - Deactivate account (only if balance is zero or less)
  - Reactivate previously deactivated account
  - Prevent duplicate account numbers
  - Prevent duplicate account names
  - Account numbers must be integers only
  - Search by account number or name
  - Filter by category, subcategory, active/inactive status
  - Event log records every create, update, activate, and deactivate action
  - Event log stores before and after JSON snapshots with user ID and timestamp
  - Chart of Accounts event log view page (filtered to ChartAccounts table)
  - Administrator has full CRUD; Manager and Accountant have view-only access
- **Definition of Done:** Administrator can add, edit, deactivate, and reactivate accounts; all validation rules enforced; duplicate detection works; event log captures all changes with before/after snapshots; Manager/Accountant can view accounts but not modify.

#### Sprint 3 - Journalizing and Ledger Module

- **Goal:** Journal entry creation, submission, manager approval workflow, ledger posting, and post reference navigation.
- **Features:**
  - Create journal entry with date, description, and multiple debit/credit lines
  - Dynamic line addition and removal on the creation form
  - Account selection from dropdown of active chart of accounts
  - Debit/credit balance enforcement (totals must match before save)
  - Validation: at least one debit line and one credit line required
  - Validation: no single line can have both debit and credit amounts
  - Validation: all referenced accounts must be active
  - Zero-amount lines automatically filtered out
  - Journal entry status workflow: Pending -> Approved/Rejected -> Posted
  - Manager approval page with entry details and balance verification
  - Manager rejection with required comment explaining the reason
  - Approved entries can be posted to the ledger by manager
  - Posting creates LedgerEntry records for each affected account
  - Posting calculates running balance using AccountingMath.SignedImpact()
  - Posting updates ChartAccount debit, credit, and balance totals
  - Posting runs within a database transaction for consistency
  - Post reference (PR) link in ledger navigates back to source journal entry
  - Ledger view by account with date range and amount filtering
  - Ledger view by journal entry showing all affected accounts
  - File attachment upload on journal entries (PDF, DOC, DOCX, XLS, XLSX, CSV, JPG, JPEG, PNG)
  - 25 MB maximum file upload size
  - Event logging for all posting and approval actions
  - Journal entry list with status, date, and search filtering
- **Definition of Done:** Accountants and Managers can create balanced journal entries; Managers can approve/reject with comments; approved entries can be posted to ledger; ledger shows running balances; post reference links work; file attachments upload and display correctly.

#### Sprint 4 - Adjusting Entries and Financial Reports

- **Goal:** Adjusting journal entries and all four financial reports with date range selection.
- **Features:**
  - Adjusting entries (special journal entries for period-end adjustments)
  - Trial balance report (all accounts with non-zero balances, verify debits = credits)
  - Income statement (revenue minus expenses for a given period)
  - Balance sheet (assets = liabilities + equity at a point in time)
  - Retained earnings statement (beginning RE + net income - dividends)
  - Date range selection for all reports
  - Generate, view, save, email, and print options for reports
- **Definition of Done:** All financial reports generate correctly for any date range; adjusting entries flow through to reports; trial balance debits equal credits; balance sheet balances.

#### Sprint 5 - Dashboard and Ratio Analysis

- **Goal:** Landing page dashboard with financial ratio analysis and color-coded health indicators.
- **Features:**
  - Financial ratio calculations displayed on landing page/dashboard
  - Color-coded indicators: green (healthy), yellow (borderline), red (out of range)
  - Role-appropriate navigation buttons on landing page
  - Notification section for pending approvals and other alerts
  - Consistent visual design with rest of application
- **Definition of Done:** Dashboard displays all required ratios with green/yellow/red indicators; notifications show relevant alerts per role; landing page provides quick access to role-appropriate features.

### 3.3 Risk Management

| Risk ID | Risk Description | Probability | Impact | Mitigation Strategy |
|---------|------------------|-------------|--------|---------------------|
| R-001 | Team member unavailability during a critical sprint | Medium | High | Document all work thoroughly so any team member can pick up any task; maintain clear code organization and comments |
| R-002 | Database incompatibility between Windows (SQL Server) and macOS (SQLite) | High | Medium | Use EnsureCreated() for local development; document Mac-specific setup steps; keep database schema compatible with both providers |
| R-003 | Scope creep beyond sprint requirements | Medium | Medium | Strictly follow the sprint requirements list; no feature additions without full team agreement; defer nice-to-have features to later sprints |
| R-004 | Integration failures between modules across sprints | Medium | High | Run full compilation check after every feature addition; test cross-module workflows after each sprint completion |
| R-005 | EF Core migration conflicts when multiple team members modify schema | Medium | Medium | Coordinate all schema changes as a team; only one person generates migrations at a time; pin dotnet-ef tool to version 8.0.x |
| R-006 | Loss of work due to missing or incomplete git commits | Low | High | Commit after every working feature; push to GitHub at least daily; write descriptive commit messages |
| R-007 | Misunderstanding of accounting domain requirements | Medium | High | Cross-reference sprint requirements document regularly; consult accounting resources when implementing financial calculations |
| R-008 | Password/security feature complexity causes delays | Medium | Medium | Use ASP.NET Identity's built-in features wherever possible; implement custom validators (PasswordHistoryValidator, StartsWithLetterPasswordValidator) as separate injectable services |
| R-009 | File upload functionality introduces security vulnerabilities | Low | High | Restrict allowed file extensions; generate random stored filenames; enforce maximum file size (25 MB); store uploads outside web-accessible paths when possible |
| [FILL IN: Additional risks identified by your team] | | | | |

### 3.4 Communication Plan

| Communication Type | Frequency | Participants | Medium |
|-------------------|-----------|--------------|--------|
| Sprint planning | Once per sprint start | Full team | [FILL IN: Discord/Teams/in-person] |
| Progress check-in | [FILL IN: Daily/twice weekly/weekly] | Full team | [FILL IN: Discord/Teams/text group] |
| Code review | After each major feature | Relevant members | GitHub (commit review) |
| Issue tracking | As needed | Full team | [FILL IN: GitHub Issues / Discord channel / other] |
| Instructor updates | Per course schedule | Team Lead + Instructor | [FILL IN: Email / in-class / office hours] |
| Sprint retrospective | End of each sprint | Full team | [FILL IN: Discord/Teams/in-person] |

---

## 4. Technical Process

### 4.1 Development Methodology

Agile/Scrum with sprint-based incremental delivery. Each of the five sprints targets a specific functional module and produces a working, integrated increment of the system. All features are developed on the master branch and committed after successful compilation verification. The team does not use a pull request workflow; instead, developers push directly to master after confirming their code compiles and does not break existing functionality.

### 4.2 Development Environment

| Component | Tool / Technology | Version |
|-----------|-------------------|---------|
| Language | C# | 12 (.NET 8) |
| Framework | ASP.NET Core Razor Pages | 8.0 |
| ORM | Entity Framework Core | 8.0.23-8.0.24 |
| Authentication | ASP.NET Identity (Identity.EntityFrameworkCore, Identity.UI) | 8.0.24 |
| Database (Windows) | SQL Server LocalDB | Included with Visual Studio |
| Database (macOS) | SQLite | Via Microsoft.EntityFrameworkCore.Sqlite 8.0.23 |
| IDE | Visual Studio 2022 / Visual Studio Code | [FILL IN: Specific version] |
| Version Control | Git + GitHub | Latest |
| Runtime | .NET 8 SDK | 8.x |
| OS (Primary) | Windows 11 | |
| OS (Secondary) | macOS | [FILL IN: If applicable] |
| CSS Framework | Bootstrap | 5.x (bundled in wwwroot/lib) |
| JavaScript Libraries | jQuery, jQuery Validation, jQuery Validation Unobtrusive | Bundled in wwwroot/lib |

### NuGet Package Inventory

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore | 8.0.24 | Developer-friendly database error pages during development |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 8.0.24 | EF Core integration for ASP.NET Identity (user/role storage) |
| Microsoft.AspNetCore.Identity.UI | 8.0.24 | Pre-built Identity UI pages (Login, Register, Manage) scaffolded and customized |
| Microsoft.EntityFrameworkCore.Sqlite | 8.0.23 | SQLite database provider for EF Core (macOS local development) |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.24 | SQL Server database provider for EF Core (primary development and production) |
| Microsoft.EntityFrameworkCore.Tools | 8.0.24 | EF Core CLI tools for migrations (Add-Migration, Update-Database) |
| Microsoft.VisualStudio.Web.CodeGeneration.Design | 8.0.23 | Scaffolding tool for generating Razor Pages and Identity pages |

### 4.3 Version Control Strategy

- **Single branch model:** All development occurs on the `master` branch. This was chosen for simplicity given the team size and direct collaboration model.
- **Commit protocol:** All changes must compile successfully before being pushed. Developers verify by running `dotnet build` locally before committing.
- **Direct pushes:** The team agreed to push directly to master without pull requests, relying on compilation verification and regular communication to prevent conflicts.
- **Commit messages:** Should describe the feature or fix being added (e.g., "Sprint 1 complete", "Add missing CanPost property to DetailsModel").
- **Local configuration protection:** Mac developers using SQLite override the database provider in their local Program.cs. These local changes are protected using `git update-index --skip-worktree` to prevent Mac-specific settings from being pushed to the shared repository.
- **Migration management:** Only one team member generates EF Core migrations at a time to prevent conflicts. The dotnet-ef tool is pinned to version 8 via .config/dotnet-tools.json.

### 4.4 Coding Standards

- **Naming conventions:** PascalCase for classes, methods, and public properties; camelCase for local variables and private fields (prefixed with underscore, e.g., `_db`).
- **Page Model pattern:** Every Razor Page consists of a .cshtml view file and a corresponding .cshtml.cs page model file. All HTTP handling logic resides in the page model.
- **Database access:** All database operations go through the EF Core `ApplicationDbContext` injected via dependency injection. No raw SQL is used.
- **Validation:** Both client-side (jQuery Validation via data annotations) and server-side (ModelState validation in page models). Error messages displayed in red.
- **Authorization:** Every page model specifies its access level via `[Authorize]` or `[Authorize(Roles = "...")]` attributes.
- **Service pattern:** Reusable business logic is extracted into services (PostingService, EventLogger, DbEmailSender, AccountingMath) registered in the DI container.
- **DateTime handling:** All timestamps stored in UTC (DateTime.UtcNow). Converted to local time only for display purposes.
- **Decimal precision:** All currency/financial values use `decimal(18,2)` to prevent floating-point errors.

---

## 5. Work Package Breakdown

### Sprint 1 - User Interface and Authentication Module

- Task 1.1: Scaffold ASP.NET Identity pages (Login, Register, Logout, ForgotPassword, ResetPassword, ChangePassword)
- Task 1.2: Create three roles (Administrator, Manager, Accountant) in Program.cs startup seed
- Task 1.3: Seed default administrator account (admin@local.test)
- Task 1.4: Configure password policy (min 8 chars, digit, special char, starts with letter)
- Task 1.5: Implement StartsWithLetterPasswordValidator custom validator
- Task 1.6: Implement PasswordHistoryValidator to prevent reuse of last 5 passwords
- Task 1.7: Create PasswordHistory model and migration
- Task 1.8: Create UserSecurity model with password expiration (90 days), suspension fields, security question/answer
- Task 1.9: Configure account lockout (3 attempts, 30-minute duration) in Identity options
- Task 1.10: Add login checks for IsActive, suspension window, and password expiry
- Task 1.11: Add 3-day password expiry warning on login
- Task 1.12: Create AccessRequest model and public RequestAccess page
- Task 1.13: Create Admin/AccessRequests page with approve/reject workflow
- Task 1.14: Implement username auto-generation (first initial + last name + MMYY)
- Task 1.15: Create Admin/Users/Index page with search, role filter, and status display
- Task 1.16: Create Admin/Users/Edit page for role assignment and suspension
- Task 1.17: Create Admin/EditUser page with comprehensive user management (role, security, email)
- Task 1.18: Create Admin/ExpiredPasswords page listing users with expired passwords
- Task 1.19: Create Admin/SuspendUser page for suspension/deactivation
- Task 1.20: Implement DbEmailSender service (stores emails in SentEmails table)
- Task 1.21: Create Admin/EmailOutbox and EmailOutboxDetails pages
- Task 1.22: Create SecurityAnswerHasher (SHA-256 hashing for security answers)
- Task 1.23: Implement ForgotPassword flow with security question verification
- Task 1.24: Create custom ForgotPasswordCustom page under Pages/Account
- Task 1.25: Create _LoginPartial showing logged-in username and logout button
- Task 1.26: Create _Layout.cshtml with navigation bar, role-based menu items, and brand

### Sprint 2 - Chart of Accounts Module

- Task 2.1: Create ChartAccount model with all fields (name, number, description, normal side, category, subcategory, initial balance, debit, credit, balance, order code, statement, comment, is active)
- Task 2.2: Create NormalSide enum (Debit, Credit) and AccountCategory enum (Asset, Liability, Equity, Revenue, Expense)
- Task 2.3: Generate EF Core migration for ChartAccounts table
- Task 2.4: Create ChartOfAccounts/Index page with account listing
- Task 2.5: Create ChartOfAccounts/Create page (Administrator only)
- Task 2.6: Implement duplicate account number validation
- Task 2.7: Implement duplicate account name validation
- Task 2.8: Create ChartOfAccounts/Edit page (Administrator only)
- Task 2.9: Implement deactivation with balance-check constraint (balance must be zero)
- Task 2.10: Implement reactivation of previously deactivated accounts
- Task 2.11: Add search functionality (by account number or name)
- Task 2.12: Add filter functionality (by category, subcategory, active/inactive status)
- Task 2.13: Create EventLog model and EventAction enum
- Task 2.14: Create EventLogger service for recording before/after JSON snapshots
- Task 2.15: Generate EF Core migration for EventLogs table
- Task 2.16: Integrate event logging into ChartOfAccounts Create, Edit, Deactivate, and Activate actions
- Task 2.17: Create ChartOfAccounts/Logs page filtered to ChartAccounts table
- Task 2.18: Create Admin/EventLogs/Index page with pagination and filtering
- Task 2.19: Create Admin/EventLogs/View page with formatted JSON display
- Task 2.20: Set IsAdmin property on ChartOfAccounts/Index to control Add/Edit button visibility

### Sprint 3 - Journalizing and Ledger Module

- Task 3.1: Create JournalEntry model with status workflow (Pending, Approved, Rejected, Posted)
- Task 3.2: Create JournalLine model with debit/credit amounts and account reference
- Task 3.3: Create JournalAttachment model for file metadata storage
- Task 3.4: Create LedgerEntry model with running balance tracking
- Task 3.5: Generate EF Core migration for journal and ledger tables
- Task 3.6: Create Journal/Create page with dynamic multi-line form
- Task 3.7: Implement debit/credit balance validation (totals must match)
- Task 3.8: Implement validation for at least one debit and one credit line
- Task 3.9: Implement validation preventing both debit and credit on same line
- Task 3.10: Implement validation ensuring all referenced accounts are active
- Task 3.11: Implement zero-amount line filtering before save
- Task 3.12: Create Journal/Index page with status, date, and search filtering
- Task 3.13: Create Journal/Details page with lines, totals, and attachment list
- Task 3.14: Create Journal/Approve page (Manager only) with approve/reject actions
- Task 3.15: Implement rejection with required manager comment
- Task 3.16: Create AccountingMath.SignedImpact() for proper debit/credit balance calculation
- Task 3.17: Create PostingService for transaction-safe ledger posting
- Task 3.18: Implement posting: create LedgerEntry records, update ChartAccount balances
- Task 3.19: Implement posting within database transaction for consistency
- Task 3.20: Create Ledger/Index page showing account ledger with date/amount filtering
- Task 3.21: Create Ledger/ByJournal page showing all accounts affected by a posted entry
- Task 3.22: Implement post reference (PR) navigation from ledger to source journal entry
- Task 3.23: Implement file upload on Journal/Details page
- Task 3.24: Configure 25 MB maximum upload size in Program.cs FormOptions
- Task 3.25: Restrict file types to PDF, DOC, DOCX, XLS, XLSX, CSV, JPG, JPEG, PNG
- Task 3.26: Implement random filename generation for stored files
- Task 3.27: Integrate event logging for approval, rejection, and posting actions

### Sprint 4 - Adjusting Entries and Financial Reports

- Task 4.1: Implement adjusting entries (special journal entries for period-end)
- Task 4.2: Create trial balance report page (accounts with non-zero balances, verify debits = credits)
- Task 4.3: Create income statement report page (revenue - expenses for date range)
- Task 4.4: Create balance sheet report page (assets = liabilities + equity at a date)
- Task 4.5: Create retained earnings statement page (beginning RE + net income - dividends)
- Task 4.6: Implement date range selection for all financial reports
- Task 4.7: Implement report generation logic using ledger data
- Task 4.8: Add save, print, and email options for reports
- Task 4.9: Integrate report pages into navigation menu

### Sprint 5 - Dashboard and Ratio Analysis

- Task 5.1: Implement financial ratio calculations from ledger/account data
- Task 5.2: Define green/yellow/red thresholds for each ratio
- Task 5.3: Create color-coded ratio display on dashboard/landing page
- Task 5.4: Add role-appropriate notification section to landing page
- Task 5.5: Add quick-access navigation buttons per role on dashboard
- Task 5.6: Final integration testing across all modules
- Task 5.7: Cross-browser compatibility testing (Chrome, Firefox, Edge, Safari)
- Task 5.8: Final documentation review and completion

---

*End of Software Project Management Plan*
