# Software Requirements Specification (SRS)

## Group 7 Accounting System

**Course:** SWE4713 - Software Engineering | **Team:** Group 7 | **Version:** 1.0 | **Date:** February 15, 2026

---

## 1. Introduction

### 1.1 Purpose

This Software Requirements Specification (SRS) formally defines all functional and non-functional requirements for the Group 7 Accounting System. It serves as the binding contract between the development team and the course instructor regarding what the system will do, how it will behave, and what constraints it must satisfy. All implementation decisions should trace back to requirements documented here.

### 1.2 Scope

The Group 7 Accounting System is a web-based accounting application built with ASP.NET Core 8 Razor Pages. It supports three user roles (Administrator, Manager, Accountant) and provides modules for:

- User authentication and role-based authorization
- User management with access request workflow
- Chart of accounts management with validation and event logging
- Journal entry creation, approval workflow, and ledger posting
- Financial report generation (trial balance, income statement, balance sheet, retained earnings)
- Dashboard with financial ratio analysis and notifications

The system uses Entity Framework Core for data access, ASP.NET Identity for authentication, SQL Server (or SQLite for macOS development) for storage, and Bootstrap 5 for the frontend interface.

### 1.3 Definitions

| Term | Definition |
|------|------------|
| Chart of Accounts | A categorized master list of all accounts used by the organization, each identified by a unique account number and name |
| Journal Entry | A recorded accounting transaction consisting of one or more debit and credit lines that must balance |
| Ledger | A chronological record of all transactions affecting a specific account, showing a running balance |
| Debit | An entry on the left side of an account; increases asset and expense accounts, decreases liability, equity, and revenue accounts |
| Credit | An entry on the right side of an account; increases liability, equity, and revenue accounts, decreases asset and expense accounts |
| Normal Side | The side (debit or credit) on which an increase to an account is recorded; determines how the balance is calculated |
| Trial Balance | A report listing all accounts with non-zero balances, used to verify that total debits equal total credits across the system |
| Income Statement | A financial report showing revenue minus expenses over a specified period, resulting in net income or net loss |
| Balance Sheet | A financial report showing the organization's assets, liabilities, and equity at a specific point in time; assets must equal liabilities plus equity |
| Retained Earnings | The cumulative net income retained in the business after dividends; reported on the retained earnings statement |
| Posting | The process of transferring an approved journal entry's debit and credit amounts to the corresponding account ledgers |
| Adjusting Entry | A journal entry made at the end of an accounting period to update account balances for accrued or deferred items |
| Post Reference (PR) | A clickable reference number in the ledger that links back to the originating journal entry |
| Event Log | An audit trail that records every data modification with before-and-after JSON snapshots, user identification, and timestamp |
| Access Request | A form submitted by a prospective user requesting an account; reviewed and approved or rejected by an administrator |
| Normal Side Impact | The calculation determining whether a debit or credit increases or decreases an account's balance based on its normal side |
| Password History | A record of previously used password hashes, used to prevent password reuse |
| Account Lockout | A security mechanism that temporarily disables login after a configurable number of failed authentication attempts |
| Security Question | A user-configured question and hashed answer used as secondary verification during password recovery |

### 1.4 References

- SWE4713 Project Specification Document
- Sprint Requirements List (Accounting_Software_-_Sprint_List)
- ASP.NET Core 8 Documentation (https://learn.microsoft.com/aspnet/core)
- Entity Framework Core Documentation (https://learn.microsoft.com/ef/core)
- ASP.NET Identity Documentation (https://learn.microsoft.com/aspnet/core/security/authentication/identity)

### 1.5 Overview

This document is organized as follows: Section 2 provides an overall description of the product including user classes and operating environment. Section 3 specifies all functional requirements organized by module. Section 4 covers non-functional requirements (security, performance, usability, reliability, maintainability). Section 5 defines external interface requirements.

---

## 2. Overall Description

### 2.1 Product Perspective

The Group 7 Accounting System is a standalone web-based multi-user accounting application. It is accessed entirely through a web browser with no client-side installation required. The backend is hosted on a web server running the .NET 8 runtime, with all data stored in a SQL Server (or SQLite) database on the server side. The application is self-contained and does not integrate with external accounting systems, banking APIs, or third-party services beyond its own email outbox functionality.

### 2.2 Product Functions

Major functional areas of the system:

- **User Authentication:** Login with username/email and password, logout, password reset via security questions
- **User Management:** Administrator creates users via access request approval, assigns roles, activates/deactivates/suspends users
- **Password Security:** Complexity enforcement, expiration (90 days), history (prevent reuse of 5), lockout (3 attempts)
- **Chart of Accounts:** Administrator adds, edits, deactivates, reactivates accounts; all roles can view and search/filter
- **Journal Entry Creation:** Managers and accountants create multi-line entries with balanced debits and credits
- **Approval Workflow:** Managers approve or reject pending journal entries with comments
- **Ledger Posting:** Approved entries are posted to ledger accounts, creating running balance records
- **File Attachments:** Source documents attached to journal entries (PDF, Word, Excel, CSV, images)
- **Event Logging:** Complete audit trail with before/after JSON snapshots for every data modification
- **Email Outbox:** All system-generated emails stored in database for administrator review
- **Financial Reports:** Trial balance, income statement, balance sheet, retained earnings statement
- **Dashboard:** Role-aware notifications, module navigation, financial ratio analysis with color indicators

### 2.3 User Classes and Characteristics

**Administrator:** The Administrator has the highest level of system access. This user manages all other user accounts - creating new accounts through the access request approval process, assigning roles, activating or deactivating accounts, suspending users for specified date ranges, and resetting locked accounts. The Administrator is the only role that can add new accounts to the chart of accounts or edit existing account details. Administrators can also view the complete event log audit trail, the email outbox, and the expired passwords report. A default Administrator account (admin@local.test) is seeded at application startup.

**Manager:** The Manager oversees the financial workflow. Managers can view the chart of accounts in read-only mode. They can create journal entries and, critically, are the only role authorized to approve or reject journal entries submitted by accountants (or themselves). When rejecting an entry, the Manager must provide a written reason. Approved entries can be posted to the ledger by the Manager. Managers can view the event log detail page and generate financial reports. The Manager's dashboard highlights pending approvals and approved-but-not-yet-posted entries.

**Accountant:** The Accountant handles day-to-day transaction recording. Accountants can view the chart of accounts in read-only mode. They create and submit journal entries for Manager approval, can attach source documents to entries, and can view the status of their submitted entries (Pending, Approved, Rejected, Posted). Accountants can view the ledger but cannot approve entries, post to the ledger, or generate financial reports. When an entry is rejected, the Accountant can view the Manager's rejection comment and create a corrected new entry.

### 2.4 Operating Environment

- **Client:** Any modern web browser (Google Chrome 100+, Mozilla Firefox 100+, Microsoft Edge 100+, Safari 15+) running on a desktop computer, laptop, or tablet
- **Server:** Windows Server with IIS or Kestrel, or any host capable of running the .NET 8 runtime
- **Database:** SQL Server (production/Windows development) or SQLite (macOS local development)
- **Network:** Internet connection required for browser-to-server communication
- **Email:** Database outbox pattern (DbEmailSender) stores all emails in SentEmails table; no external SMTP server required for current implementation

### 2.5 Design and Implementation Constraints

- Built on ASP.NET Core 8 Razor Pages - all pages follow the Page Model pattern
- Must use Entity Framework Core for all data access (no raw SQL)
- Must use ASP.NET Identity for authentication and authorization
- Passwords must be minimum 8 characters, start with a letter, contain at least one digit and one special character
- Maximum 3 failed login attempts before 30-minute account lockout
- Password expiration set to 90 days with 3-day advance warning
- Last 5 passwords cannot be reused (tracked via PasswordHistory table)
- Debits must equal credits before a journal entry can be saved
- Accounts cannot be deleted from the chart of accounts - only deactivated
- Accounts with a balance greater than zero cannot be deactivated
- Account numbers must be integers only (no decimals, no alphabetic characters)
- No duplicate account numbers or account names allowed
- Only the Administrator role can add or edit chart of accounts entries
- Only the Manager role can approve or reject journal entries
- Journal entries cannot be edited or deleted once submitted
- Rejected entries require a manager comment explaining the rejection
- File uploads limited to 25 MB, restricted to: PDF, DOC, DOCX, XLS, XLSX, CSV, JPG, JPEG, PNG
- Error messages must be stored in the AppErrorMessages database table and displayed in red
- All timestamps stored in UTC, converted to local time for display only
- All currency values use decimal(18,2) precision

---

## 3. Specific Functional Requirements

### 3.1 User Management Module

**FR-001:** The system shall provide a login page where users enter a username or email address and a password to authenticate.

**FR-002:** The password field on the login page shall be masked (hidden characters) during entry.

**FR-003:** The system shall allow users to log in using either their username or their email address (case-insensitive email matching).

**FR-004:** On successful authentication, the system shall redirect the user to the Home/Dashboard page appropriate for their role.

**FR-005:** The system shall display a "Forgot your password?" link on the login page that initiates the password recovery flow.

**FR-006:** The forgot password flow shall require the user to provide their username, email address, and the correct answer to their security question before generating a password reset link.

**FR-007:** Security question answers shall be hashed using SHA-256 before storage and verified using fixed-time comparison to prevent timing attacks.

**FR-008:** The password reset flow shall generate a token-based reset link and store it in the SentEmails table (email outbox).

**FR-009:** When resetting a password, the system shall check the last 10 password hashes and reject any password that matches a previously used one.

**FR-010:** After a successful password reset, the system shall update PasswordLastChangedAt and set PasswordExpiresAt to 90 days from now, and unlock the user's account.

**FR-011:** The system shall redirect the standard registration page to a custom Access Request page (/Public/RequestAccess) where prospective users submit their first name, last name, email, address, and date of birth.

**FR-012:** Access requests shall be stored in the AccessRequests table with status Pending, and a notification email shall be logged to the SentEmails table.

**FR-013:** The Administrator shall be able to view all access requests on the Admin/AccessRequests page, ordered by submission date.

**FR-014:** When approving an access request, the system shall automatically generate a username using the format: first initial + full last name + two-digit month + two-digit year of creation (all lowercase).

**FR-015:** When approving an access request, the system shall create a new user account with a temporary password ("Temp!234"), create a PasswordHistory record, create a UserSecurity record with 90-day password expiration, and assign a role (defaulting to Accountant).

**FR-016:** When rejecting an access request, the Administrator shall be required to provide a comment explaining the rejection reason, and a notification email shall be logged.

**FR-017:** The Administrator shall be able to assign one of three roles to any user: Administrator, Manager, or Accountant.

**FR-018:** The Administrator shall be able to activate or deactivate user accounts by setting the IsActive flag in the UserSecurity table.

**FR-019:** The Administrator shall be able to suspend a user account by setting SuspendedFrom and SuspendedUntil dates, temporarily preventing login during that window.

**FR-020:** The login process shall check the following conditions in order: (1) user exists, (2) IsActive is true, (3) current time is not within the suspension window, (4) password is not expired, (5) credentials are valid.

**FR-021:** If the password will expire within 3 days, the system shall display a warning message after successful login.

**FR-022:** The system shall lock a user's account after 3 consecutive failed login attempts, setting a 30-minute lockout duration.

**FR-023:** The system shall enforce password complexity: minimum 8 characters, must start with a letter, must contain at least one digit (0-9), must contain at least one special character.

**FR-024:** The system shall track password history and prevent reuse of the last 5 passwords (enforced via PasswordHistoryValidator).

**FR-025:** The Administrator shall have access to a Users report page showing all users with their username, email, role, active status, lockout status, and password expiry date, with search and role filtering.

**FR-026:** The Administrator shall have access to an Expired Passwords report page listing all users whose PasswordExpiresAt date has passed.

**FR-027:** The Administrator shall be able to send an email to any user from the EditUser page, with the email stored in the SentEmails table.

**FR-028:** The Administrator shall be able to unlock a locked user account by resetting the lockout end date and failed attempt count.

**FR-029:** The system shall display the logged-in user's username on every page via the _LoginPartial component in the navigation bar.

**FR-030:** The system shall display the application brand/logo ("AS" mark and "AccountingSystem" text) on every page in the navigation bar.

**FR-031:** The system shall seed three roles (Administrator, Manager, Accountant) and a default administrator account (admin@local.test / Admin!234) on first startup.

### 3.2 Chart of Accounts Module

**FR-032:** The Administrator shall be able to add a new account to the chart of accounts with the following fields: Account Name (required, max 120 chars), Account Number (required, integer), Description (optional, max 500 chars), Normal Side (Debit or Credit), Category (Asset, Liability, Equity, Revenue, or Expense), Subcategory (optional, max 100 chars), Initial Balance (decimal, min 0), Order Code (required, max 10 chars), Statement (IS, BS, or RE), and Comment (optional, max 500 chars).

**FR-033:** When creating a new account, the system shall set Balance equal to InitialBalance, AddedAtUtc to the current UTC time, and AddedByUserId to the current user.

**FR-034:** The system shall reject any new account if its Account Number matches an existing account's number (duplicate prevention).

**FR-035:** The system shall reject any new account if its Account Name matches an existing account's name (duplicate prevention).

**FR-036:** Account numbers must be integers only. The system shall not accept decimal values or alphabetic characters.

**FR-037:** The Administrator shall be able to edit an existing account's details (name, description, normal side, category, subcategory, order code, statement, comment). Duplicate name/number validation shall apply excluding the account being edited.

**FR-038:** The Administrator shall be able to deactivate an account by setting IsActive to false. The system shall prevent deactivation if the account's balance is greater than zero.

**FR-039:** The Administrator shall be able to reactivate a previously deactivated account by setting IsActive to true.

**FR-040:** All authenticated users (Administrator, Manager, Accountant) shall be able to view the chart of accounts with columns: account number, account name, category, subcategory, normal side, balance, and active status.

**FR-041:** Manager and Accountant roles shall have view-only access to the chart of accounts - they cannot add, edit, deactivate, or reactivate accounts.

**FR-042:** The chart of accounts page shall provide search functionality allowing users to search by account number (exact integer match) or account name (partial text match).

**FR-043:** The chart of accounts page shall provide filter functionality by account category, subcategory (partial match), and active/inactive status.

**FR-044:** Clicking an account name on the chart of accounts shall navigate to the ledger page for that specific account.

**FR-045:** Every create, update, deactivate, and activate action on the chart of accounts shall be recorded in the EventLogs table with: table name ("ChartAccounts"), record ID, action type, before JSON snapshot, after JSON snapshot, user ID, and UTC timestamp.

**FR-046:** A dedicated Chart of Accounts Logs page shall display event logs filtered to the ChartAccounts table, with optional filtering by record ID, ordered by timestamp descending.

### 3.3 Journalizing and Ledger Module

**FR-047:** Managers and Accountants shall be able to create a new journal entry with: entry date (defaults to today), description (optional, max 200 chars), and one or more journal lines.

**FR-048:** Each journal line shall consist of: a reference to an active chart of accounts entry, a debit amount (decimal), a credit amount (decimal), and an optional memo (max 200 chars).

**FR-049:** The journal entry creation form shall initialize with 2 blank lines and allow the user to dynamically add more lines or remove existing lines.

**FR-050:** The system shall require at least one line with a debit amount greater than zero and at least one line with a credit amount greater than zero.

**FR-051:** The system shall reject any line where both debit and credit amounts are greater than zero (a line can be either a debit or a credit, not both).

**FR-052:** The system shall validate that the total of all debit amounts equals the total of all credit amounts before allowing the entry to be saved.

**FR-053:** The system shall validate that all chart of accounts referenced in journal lines are currently active.

**FR-054:** Lines with both debit and credit equal to zero shall be automatically filtered out before saving.

**FR-055:** A newly created journal entry shall have status set to Pending, CreatedByUserId set to the current user, and CreatedAtUtc set to the current UTC time.

**FR-056:** All authenticated users shall be able to view the journal entries list page with filtering by: status (Pending, Approved, Rejected, Posted), date range (EntryDate), and search (by date, debit/credit amount, or account name).

**FR-057:** The journal entries list shall display entries ordered by entry date descending, limited to the most recent 200 entries.

**FR-058:** All authenticated users shall be able to view the details of any journal entry, including its lines (account, debit, credit, memo), total debits and credits, status, and attached files.

**FR-059:** Only users with the Manager role shall be able to access the Journal Approve page to review and act on pending entries.

**FR-060:** When approving a journal entry, the Manager action shall set the status to Approved, record the ApprovedByUserId and ApprovedAtUtc, and clear any existing ManagerComment.

**FR-061:** When rejecting a journal entry, the Manager shall be required to provide a comment (ManagerComment) explaining the rejection reason. The status shall be set to Rejected.

**FR-062:** Only journal entries with status Pending and balanced debits/credits can be approved or rejected.

**FR-063:** Approved journal entries can be posted to the ledger by a Manager. Posting shall only be allowed when: status is Approved, the entry has at least one line, and total debits equal total credits.

**FR-064:** When posting a journal entry, the system shall create one LedgerEntry record for each journal line, containing: ChartAccountId, EntryDate, JournalEntryId (post reference), Description, Debit, Credit, BalanceAfter (running balance), and PostedAtUtc.

**FR-065:** The BalanceAfter for each ledger entry shall be calculated using the AccountingMath.SignedImpact method: for debit-normal accounts, impact = debit - credit; for credit-normal accounts, impact = credit - debit. The new balance is the previous balance plus the impact.

**FR-066:** When posting, the system shall update each affected ChartAccount's cumulative Debit, Credit, and Balance totals.

**FR-067:** The posting operation shall execute within a database transaction. If any step fails, the entire posting is rolled back.

**FR-068:** After successful posting, the journal entry's status shall be set to Posted, PostedByUserId and PostedAtUtc shall be recorded.

**FR-069:** Posting shall be logged in the EventLogs table with before/after snapshots for both the journal entry and each affected chart account.

**FR-070:** The ledger page for a specific account shall display all ledger entries for that account, with columns: entry date, description, debit, credit, and running balance (BalanceAfter).

**FR-071:** The ledger page shall support filtering by date range and searching by amount.

**FR-072:** The ledger page shall display entries ordered by entry date, then by ledger entry ID, limited to 500 entries.

**FR-073:** Each ledger entry shall display its JournalEntryId as a clickable post reference (PR) that navigates to the source journal entry details.

**FR-074:** A "Ledger by Journal" page shall show all ledger entries created from a specific posted journal entry, with account details, debits, credits, balances, and totals.

**FR-075:** Managers and Accountants shall be able to upload file attachments on the journal entry details page. Accepted file types: .pdf, .doc, .docx, .xls, .xlsx, .csv, .jpg, .jpeg, .png.

**FR-076:** Uploaded files shall be stored in the wwwroot/uploads/journal/{journalId}/ directory with a randomly generated filename preserving the original extension.

**FR-077:** A JournalAttachment record shall be created for each upload, storing: original filename, stored filename, content type, file size in bytes, upload timestamp, and uploading user ID.

**FR-078:** The maximum file upload size shall be 25 MB, configured via ASP.NET Core FormOptions.

### 3.4 Adjusting Entries and Financial Reports Module

**FR-079:** The system shall support adjusting entries as journal entries created at the end of an accounting period to update account balances for accrued or deferred items. [PARTIALLY IMPLEMENTED - a boolean IsAdjusting flag was added to the JournalEntry model via migration AddIsAdjustingToJournalEntry. An "Adjusting Entry" checkbox is present on the journal create form. When checked, the submission email notification subject includes the word "adjusting." No separate adjusting entry workflow, page, or reporting category exists; adjusting entries use the standard journal entry approval and posting flow.]

**FR-080:** The system shall generate a Trial Balance report showing all active accounts with debit and credit balance columns, and grand totals. The report is accessible at /Reports/TrialBalance and is restricted to the Manager role.

**FR-081:** The system shall generate an Income Statement report showing accounts with Statement = "IS" (Income Statement). Revenue accounts display positive amounts; Expense accounts display negative amounts. The Net Income total is displayed in the report footer. Accessible at /Reports/IncomeStatement (Manager only).

**FR-082:** The system shall generate a Balance Sheet report showing all accounts with Statement = "BS" (Balance Sheet). Accounts are ordered by OrderCode and AccountNumber. Accessible at /Reports/BalanceSheet (Manager only). An as-of-date filter is available in the UI but does not currently filter the query data.

**FR-083:** The system shall generate a Retained Earnings Statement showing accounts with Statement = "RE" (Retained Earnings). Accessible at /Reports/RetainedEarnings (Manager only).

**FR-084:** All financial report pages include date range (From/To) or as-of-date filter inputs in the UI. [NOT IMPLEMENTED IN FINAL SUBMISSION - the date parameters are bound in page models and passed to FinancialReportService methods, but the service queries do not apply them to the database queries. Reports always reflect all-time account balances regardless of the selected date range.]

**FR-085:** Financial reports provide the following output options: Generate (click Generate button to render the report), Print (browser print via window.print()), Export CSV (downloads a formatted CSV file), and Email (stores the report notification in the SentEmails outbox table). A dedicated save-to-server or PDF export option is not implemented.

### 3.5 Dashboard and Ratio Analysis Module

**FR-086:** The system shall display a dashboard/landing page after login showing role-appropriate notifications: pending journal entry count, rejected entry count, and approved-not-yet-posted count.

**FR-087:** The dashboard shall display notification alerts for Managers when pending approvals exist (warning style) and when approved entries await posting (info style).

**FR-088:** The dashboard shall provide quick-access navigation cards to all major modules: Chart of Accounts, Journal, Ledger, and administrator-specific pages.

**FR-089:** Administrator-specific dashboard section shall show cards for: Access Requests, Expired Passwords, Suspend User, Event Logs, and Email Outbox.

**FR-090:** The dashboard shall include account management links: Manage Account, Forgot Password, and Logout.

**FR-091:** The system shall calculate and display financial ratios on the landing page with color-coded indicators: green (healthy), yellow (borderline), red (out of range). [NOT IMPLEMENTED IN FINAL SUBMISSION - the dashboard pages (Index and Dashboard) display role-aware notification counts only. No ratio calculation code, ratio display components, or color-coded indicators exist in the codebase.]

**FR-092:** When a journal entry is submitted for approval, the system shall send an email notification to all users assigned the Manager role. The notification is stored in the SentEmails outbox table. The email subject identifies the entry as either a standard or adjusting journal entry based on the IsAdjusting flag.

---

## 4. Non-Functional Requirements

### 4.1 Security (NFR-SEC)

**NFR-SEC-001:** All user passwords shall be hashed using ASP.NET Identity's built-in PBKDF2 with HMAC-SHA256 algorithm before storage. Plaintext passwords shall never be stored or logged.

**NFR-SEC-002:** Role-based authorization shall be enforced on every page using `[Authorize]` or `[Authorize(Roles="...")]` attributes in page model classes.

**NFR-SEC-003:** All POST forms shall include ASP.NET Core's anti-forgery token to prevent Cross-Site Request Forgery (CSRF) attacks.

**NFR-SEC-004:** User accounts shall be locked after 3 consecutive failed login attempts, with a 30-minute lockout duration.

**NFR-SEC-005:** Passwords shall meet minimum complexity requirements: 8+ characters, starts with a letter, contains at least one digit, contains at least one special (non-alphanumeric) character.

**NFR-SEC-006:** Password history shall prevent reuse of the last 5 previously used passwords, verified by comparing hashes stored in the PasswordHistories table.

**NFR-SEC-007:** Passwords shall expire after 90 days. Users shall receive a warning when 3 or fewer days remain before expiration. Expired passwords trigger a mandatory change prompt on login.

**NFR-SEC-008:** Security question answers shall be hashed using SHA-256 with lowercase normalization before storage. Verification shall use fixed-time comparison to prevent timing attacks.

**NFR-SEC-009:** File uploads shall be restricted to approved file types (.pdf, .doc, .docx, .xls, .xlsx, .csv, .jpg, .jpeg, .png) and a maximum size of 25 MB to prevent abuse.

**NFR-SEC-010:** Uploaded files shall be stored with randomly generated filenames to prevent directory traversal and filename collision attacks.

**NFR-SEC-011:** The default administrator credentials (admin@local.test / Admin!234) shall be changed before any production deployment.

### 4.2 Performance (NFR-PERF)

**NFR-PERF-001:** Pages shall load within 3 seconds under normal server load with a typical dataset.

**NFR-PERF-002:** The system shall support multiple concurrent users without data corruption, leveraging EF Core's optimistic concurrency and database transactions.

**NFR-PERF-003:** Financial reports shall generate within 5 seconds for typical dataset sizes.

**NFR-PERF-004:** List pages shall implement result limits (200-500 records) to prevent excessive memory usage and slow page rendering.

### 4.3 Usability (NFR-USE)

**NFR-USE-001:** The application shall maintain a consistent color scheme, layout, and navigation structure across all pages using a shared _Layout.cshtml and site.css.

**NFR-USE-002:** Every interactive button shall have a tooltip describing its purpose.

**NFR-USE-003:** A built-in Help page shall be accessible from the main navigation bar on every page.

**NFR-USE-004:** The application shall be accessible on desktop computers, laptops, and tablets through responsive Bootstrap 5 layout.

**NFR-USE-005:** The application shall function correctly on Chrome 100+, Firefox 100+, Edge 100+, and Safari 15+.

**NFR-USE-006:** The application brand mark ("AS") and name ("AccountingSystem") shall appear in the navigation bar on every page.

**NFR-USE-007:** The currently logged-in user's name shall be displayed in the top navigation bar on every page via the _LoginPartial component.

**NFR-USE-008:** Navigation menus shall be role-aware, showing only links to pages the current user is authorized to access.

### 4.4 Reliability (NFR-REL)

**NFR-REL-001:** All user inputs shall be validated both client-side (jQuery Validation via data annotations) and server-side (ModelState validation) before saving to the database.

**NFR-REL-002:** Error messages shall be displayed in red color to clearly distinguish them from normal content.

**NFR-REL-003:** Error message templates shall be stored in the AppErrorMessages database table for centralized management.

**NFR-REL-004:** Error messages shall disappear once the user corrects the input and resubmits the form.

**NFR-REL-005:** The event log shall record every data modification with before-and-after JSON snapshots, the user who made the change, and a UTC timestamp, providing a complete audit trail.

**NFR-REL-006:** The ledger posting operation shall run within a database transaction. If any step fails, all changes are rolled back, preventing partial postings.

### 4.5 Maintainability (NFR-MAIN)

**NFR-MAIN-001:** The codebase shall be organized by feature using the Razor Pages convention: each page has a .cshtml view and a .cshtml.cs page model, grouped into folders by module (Admin, ChartOfAccounts, Journal, Ledger, Help, Public).

**NFR-MAIN-002:** Entity Framework Core shall abstract all database operations. No raw SQL shall be used in the application code.

**NFR-MAIN-003:** All source code shall be version-controlled in GitHub under the repository AD-AccountingSystem-Group7.

**NFR-MAIN-004:** Reusable business logic shall be extracted into injectable services (PostingService, EventLogger, DbEmailSender, AccountingMath) registered in the dependency injection container.

**NFR-MAIN-005:** All custom validators (StartsWithLetterPasswordValidator, PasswordHistoryValidator) shall be implemented as separate classes registered as scoped services.

---

## 5. External Interface Requirements

### 5.1 User Interfaces

| Page | URL Route | Accessible By | Description |
|------|-----------|---------------|-------------|
| Login | /Identity/Account/Login | Public | Username/email and password authentication |
| Logout | /Identity/Account/Logout | Authenticated | Signs out user and redirects to login |
| Register (redirects) | /Identity/Account/Register | Public | Redirects to /Public/RequestAccess |
| Request Access | /Public/RequestAccess | Public | New user access request form |
| Forgot Password (Identity) | /Identity/Account/ForgotPassword | Public | Password reset via security question (Identity scaffolded) |
| Forgot Password (Custom) | /Account/ForgotPasswordCustom | Public | Custom password reset with security question |
| Reset Password | /Identity/Account/ResetPassword | Public (with token) | Set new password using reset token |
| Change Password | /Identity/Account/Manage/ChangePassword | Authenticated | Change password for logged-in user |
| Home / Index | / | Authenticated | Landing page with notifications and quick links |
| Dashboard | /Dashboard | Authenticated | Table of contents with module cards and notifications |
| Chart of Accounts | /ChartOfAccounts | Authenticated | List all accounts with search/filter |
| Chart of Accounts - Create | /ChartOfAccounts/Create | Administrator | Add new account form |
| Chart of Accounts - Edit | /ChartOfAccounts/Edit/{id} | Administrator | Edit existing account |
| Chart of Accounts - Logs | /ChartOfAccounts/Logs | Authenticated | Event logs for chart of accounts changes |
| Journal - Index | /Journal | Authenticated | List journal entries with status/date/search filters |
| Journal - Create | /Journal/Create | Manager, Accountant | Create new multi-line journal entry |
| Journal - Details | /Journal/Details/{id} | Authenticated | View journal entry details, upload attachments, post |
| Journal - Approve | /Journal/Approve/{id} | Manager | Approve or reject pending journal entry |
| Ledger - Index | /Ledger?accountId={id} | Authenticated | View account ledger with date/amount filtering |
| Ledger - By Journal | /Ledger/ByJournal/{id} | Admin, Manager, Accountant | View ledger entries from a specific posted journal |
| Admin - Access Requests | /Admin/AccessRequests | Administrator | Review and approve/reject access requests |
| Admin - Users | /Admin/Users | Administrator | List all users with search/filter/status |
| Admin - Users Edit | /Admin/Users/Edit/{userId} | Administrator | Edit user role and suspension dates |
| Admin - Edit User | /Admin/EditUser/{id} | Administrator | Comprehensive user editing (role, security, email) |
| Admin - Expired Passwords | /Admin/ExpiredPasswords | Administrator | List users with expired passwords |
| Admin - Suspend User | /Admin/SuspendUser | Administrator | Suspend or deactivate a user account |
| Admin - Event Logs | /Admin/EventLogs | Administrator | Paginated audit trail with filtering |
| Admin - Event Log View | /Admin/EventLogs/View/{id} | Administrator, Manager | View single event log detail with JSON |
| Admin - Email Outbox | /Admin/EmailOutbox | Administrator | List all sent emails with search/filter |
| Admin - Email Outbox Details | /Admin/EmailOutboxDetails/{id} | Administrator | View full email content |
| Reports - Index | /Reports | Manager | Hub page with links to all four financial reports |
| Reports - Trial Balance | /Reports/TrialBalance | Manager | Trial balance with debit/credit columns, totals, Print/Export CSV/Email |
| Reports - Income Statement | /Reports/IncomeStatement | Manager | Income statement with net income footer, Print/Export CSV/Email |
| Reports - Balance Sheet | /Reports/BalanceSheet | Manager | Balance sheet with as-of date filter, Print/Export CSV/Email |
| Reports - Retained Earnings | /Reports/RetainedEarnings | Manager | Retained earnings statement, Print/Export CSV/Email |
| Help | /Help | All (including unauthenticated) | Built-in help and guidance |
| Privacy | /Privacy | All (including unauthenticated) | Privacy policy page |
| Error | /Error | All | Error display with request ID |

### 5.2 Hardware Interfaces

The system requires standard web server hardware capable of running the .NET 8 runtime. No special hardware is required on the client side beyond a device with a web browser (desktop, laptop, or tablet). The server needs sufficient disk space for the database and uploaded file attachments.

### 5.3 Software Interfaces

| Component | Interface | Purpose |
|-----------|-----------|---------|
| Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore 8.0.24 | NuGet Package | Developer-friendly database error pages |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.24 | NuGet Package | EF Core storage providers for Identity (users, roles, claims) |
| Microsoft.AspNetCore.Identity.UI 8.0.24 | NuGet Package | Pre-built and scaffoldable Identity UI pages |
| Microsoft.EntityFrameworkCore.Sqlite 8.0.23 | NuGet Package | SQLite database provider for macOS development |
| Microsoft.EntityFrameworkCore.SqlServer 8.0.24 | NuGet Package | SQL Server database provider for production |
| Microsoft.EntityFrameworkCore.Tools 8.0.24 | NuGet Package | EF Core CLI tools for migrations |
| Microsoft.VisualStudio.Web.CodeGeneration.Design 8.0.23 | NuGet Package | Scaffolding tool for code generation |
| Bootstrap 5.x | Client-side Library | Responsive CSS framework for layout and components |
| jQuery 3.x | Client-side Library | JavaScript library for DOM manipulation |
| jQuery Validation | Client-side Library | Client-side form validation |
| jQuery Validation Unobtrusive | Client-side Library | ASP.NET Core integration for jQuery Validation |

### 5.4 Communication Interfaces

- **HTTP/HTTPS:** All browser-to-server communication uses HTTP (development) or HTTPS (production). ASP.NET Core's Kestrel server or IIS handles request routing.
- **SMTP (Future):** The system is architected to support SMTP email sending. Currently, the DbEmailSender stores all emails in the SentEmails database table (outbox pattern). To enable actual email sending, the IEmailSender implementation would be replaced with an SMTP-capable sender.
- **Database:** Communication between the application and database uses Entity Framework Core's ADO.NET providers - SqlClient for SQL Server and Microsoft.Data.Sqlite for SQLite.

---

*End of Software Requirements Specification*
