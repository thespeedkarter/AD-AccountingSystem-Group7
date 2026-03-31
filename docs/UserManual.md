# User Manual

## Group 7 Accounting System

**Version:** 1.0 | **Course:** SWE4713 - Software Engineering | **Date:** February 15, 2026

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [Getting Started](#2-getting-started)
3. [Administrator Guide](#3-administrator-guide)
4. [Manager Guide](#4-manager-guide)
5. [Accountant Guide](#5-accountant-guide)
6. [Dashboard and Financial Ratios](#6-dashboard-and-financial-ratios)
7. [Troubleshooting](#7-troubleshooting)
8. [Glossary](#8-glossary)

---

## 1. Introduction

Welcome to the Group 7 Accounting System User Manual. This guide will help you navigate and use the accounting system to manage your organization's financial records.

The Accounting System is a web-based application that you access through your internet browser - there is nothing to install on your computer. The system handles the core accounting workflow: maintaining a chart of accounts, creating and approving journal entries, posting transactions to the general ledger, and generating financial reports.

There are three types of users in the system:

- **Administrator** - Manages user accounts, maintains the chart of accounts, and oversees the entire system.
- **Manager** - Reviews and approves or rejects journal entries, posts approved entries to the ledger, and generates financial reports.
- **Accountant** - Creates journal entries, attaches supporting documents, and tracks the status of submitted entries.

Each section of this manual is organized by role so you can quickly find the instructions relevant to your work.

---

## 2. Getting Started

### 2.1 System Requirements

To use the Accounting System, you need:

- A computer, laptop, or tablet with a modern web browser installed
- A stable internet connection
- Login credentials provided by your Administrator

### 2.2 Supported Browsers

The application works on the following browsers:

- **Google Chrome** (recommended) - version 100 or newer
- **Mozilla Firefox** - version 100 or newer
- **Microsoft Edge** - version 100 or newer
- **Safari** (Mac and iPad) - version 15 or newer

### 2.3 Accessing the Application

1. Open your web browser.
2. In the address bar, type the application URL: `https://september-mother-favourite-doctors.trycloudflare.com/Identity/Account/Login?ReturnUrl=%2F`
   Note: If the above URL is unavailable, contact the system administrator for the current access address.
3. Press Enter. The login page will appear.

### 2.4 Logging In

1. On the login page, you will see two fields: **Username or Email** and **Password**.
2. Type your username (e.g., jsmith0326) or your email address in the first field.
3. Type your password in the second field. The characters will appear as dots for security.
4. Optionally, check the **Remember me** box if you are on a private computer and want to stay logged in.
5. Click the **Log in** button.
6. If your credentials are correct, you will be taken to the Home page.

**First-time login:** If your account was just created by an Administrator, you will be given a temporary password. After logging in for the first time, navigate to the Change Password page to set your own personal password.

### 2.5 Logging Out

1. Look at the top-right corner of the page. You will see your username displayed.
2. Click the **Logout** button next to your username.
3. You will be signed out and returned to the login page.

Always log out when you are finished using the system, especially on shared computers. This protects your account and the organization's financial data.

### 2.6 Forgot Your Password?

If you have forgotten your password, follow these steps:

1. On the login page, click the **Forgot your password?** link.
2. Enter your **username** in the first field.
3. Enter the **email address** associated with your account.
4. Click **Submit**.
5. If your information matches, your **security question** will be displayed (for example, "What is the name of your first pet?").
6. Type the correct answer to your security question.
7. Click **Submit** again.
8. A password reset link will be generated. Click the link or copy it into your browser.
9. On the Reset Password page, enter your new password and confirm it.
10. Click **Reset Password**.
11. You will be redirected to the login page where you can log in with your new password.

**Important:** If you do not have a security question configured, contact your Administrator to have your password reset manually.

### 2.7 Password Requirements

When creating or changing your password, it must meet all of the following rules:

- Must be **at least 8 characters** long
- Must **start with a letter** (a-z or A-Z)
- Must contain at least **one number** (0-9)
- Must contain at least **one special character** (such as !, @, #, $, %, &, *)
- **Cannot be the same** as any of your last 5 passwords

**Examples of valid passwords:**
- `Spring2026!`
- `Accounting#1`
- `MyPassword@99`

**Examples of invalid passwords:**
- `12345678` (does not start with a letter, no special character)
- `Password` (no number, no special character)
- `Ab1!` (too short - less than 8 characters)

### 2.8 What Happens If I Enter the Wrong Password Too Many Times?

If you enter the wrong password **3 times in a row**, your account will be **temporarily locked for 30 minutes**. During this time:

- You will see a message saying your account is locked.
- You will not be able to log in even with the correct password.
- After 30 minutes, the lock is automatically removed and you can try again.

If you need immediate access, contact your **Administrator**. They can unlock your account manually.

---

## 3. Administrator Guide

### 3.1 Administrator Dashboard

When you log in as an Administrator, you will see the Dashboard page. This page serves as your home base and shows:

- **Notifications** at the top - alerts about pending journal entries, rejected entries, and approved entries waiting to be posted
- **Module cards** - quick links to Chart of Accounts, Journal, and Ledger
- **Administrator section** - links to Access Requests, Expired Passwords, Suspend User, Event Logs, and Email Outbox
- **Account section** - links to Manage Account, Forgot Password, and Logout

You can also access all sections from the **navigation bar** at the top of every page. Click **Admin** in the nav bar to see the dropdown menu with all administrator tools.

### 3.2 Managing Users

#### 3.2.1 Viewing All Users

1. Click **Admin** in the navigation bar, then click **Users**.
2. The Users page displays a table of all user accounts showing: username, email, role, active status, lockout status, and password expiry date.
3. To search for a specific user, type their username or email in the **Search** field and press Enter.
4. To filter by role, select a role from the **Role** dropdown (Administrator, Manager, or Accountant).
5. To see deactivated users, check the **Show Inactive** checkbox.

#### 3.2.2 Creating a New User

New users are created through the **Access Request** workflow:

1. A prospective user visits the public **Request Access** page and submits their information (name, email, date of birth, address).
2. Click **Admin** in the navigation bar, then click **Access Requests**.
3. You will see a list of all requests. Pending requests are waiting for your action.
4. To **approve** a request:
   a. Select a **Role** to assign (Administrator, Manager, or Accountant). The default is Accountant.
   b. Click the **Approve** button next to the request.
   c. The system will automatically generate a username using the format: **first initial + last name + two-digit month + two-digit year**. For example, if John Smith requests access in March 2026, the username will be **jsmith0326**.
   d. The system creates the account with a temporary password (**Temp!234**) and sends a notification email.
5. The new user can then log in with their generated username and temporary password, and should change their password immediately.

#### 3.2.3 Editing a User's Information

1. Navigate to **Admin > Users**.
2. Click the **Edit** link next to the user you want to modify.
3. On the Edit page, you can:
   - Change the user's **role** using the dropdown
   - Set **suspension dates** (Suspended From and Suspended Until) for temporary suspension
4. Click **Save** to apply changes.

For more comprehensive editing (including security questions and sending emails), use the **Edit User** page accessible from the admin section.

#### 3.2.4 Activating or Deactivating a User

1. Navigate to **Admin > Suspend User**.
2. Select the user from the dropdown list.
3. To **deactivate**: Check the **Deactivate Account** checkbox and click Submit. The user will no longer be able to log in.
4. To **reactivate**: Uncheck the Deactivate Account checkbox, clear any suspension dates, and click Submit. The user can log in again.

#### 3.2.5 Suspending a User (Temporary)

Suspension is different from deactivation - it temporarily prevents login during a specific date range:

1. Navigate to **Admin > Suspend User**.
2. Select the user from the dropdown.
3. Set the **Suspended From** date and time (when the suspension begins).
4. Set the **Suspended Until** date and time (when the suspension ends).
5. Make sure the Deactivate checkbox is **unchecked**.
6. Click **Submit**.

The user will be unable to log in between the "from" and "until" dates. After the "until" date passes, they can log in normally again.

#### 3.2.6 Sending an Email to a User

1. Navigate to the **Edit User** page for the target user (via Admin > Users > Edit, or the comprehensive Edit User page).
2. Scroll to the **Send Email** section.
3. Enter an **Email Subject** line.
4. Enter the **Email Body** (the message you want to send).
5. Click **Send Email**.

Note: In the current system, emails are stored in the **Email Outbox** database rather than being sent externally via SMTP. The email content can be viewed in the Email Outbox.

### 3.3 Managing the Chart of Accounts

#### 3.3.1 Understanding the Chart of Accounts

The Chart of Accounts is the master list of all financial accounts your organization uses to categorize transactions. Think of it as a filing system for money - every dollar that comes in or goes out is recorded in a specific account. Common accounts include "Cash," "Accounts Receivable," "Revenue," and "Office Supplies Expense."

Accounts are organized into five categories:
- **Assets** - Things the organization owns (Cash, Equipment, Inventory)
- **Liabilities** - Things the organization owes (Accounts Payable, Loans)
- **Equity** - The owner's share of the organization (Owner's Equity, Retained Earnings)
- **Revenue** - Money earned (Sales Revenue, Service Income)
- **Expenses** - Money spent (Rent, Salaries, Utilities)

#### 3.3.2 Adding a New Account

Only Administrators can add new accounts. To add one:

1. Navigate to **Chart of Accounts** from the navigation bar.
2. Click the **Add New Account** button (only visible to Administrators).
3. Fill in the required fields:

   - **Account Name** - A clear, descriptive name (e.g., "Cash," "Accounts Receivable"). Must be unique - no two accounts can share the same name.
   - **Account Number** - A unique whole number. The first digit typically indicates the account type:
     - 1xxx = Asset accounts (e.g., 1000 = Cash)
     - 2xxx = Liability accounts (e.g., 2000 = Accounts Payable)
     - 3xxx = Equity accounts (e.g., 3000 = Owner's Equity)
     - 4xxx = Revenue accounts (e.g., 4000 = Sales Revenue)
     - 5xxx = Expense accounts (e.g., 5000 = Rent Expense)
   - **Normal Side** - Select **Debit** for Asset and Expense accounts, or **Credit** for Liability, Equity, and Revenue accounts. This determines how the account's balance is calculated.
   - **Category** - Select Asset, Liability, Equity, Revenue, or Expense.
   - **Subcategory** - Optional further classification (e.g., "Current Assets," "Fixed Assets," "Long-term Liabilities").
   - **Initial Balance** - The starting balance when the account is first created. Enter 0 if the account starts empty.
   - **Order Code** - A sort code for display ordering (e.g., "01," "02").
   - **Statement** - Which financial statement this account appears on:
     - **IS** = Income Statement (Revenue and Expense accounts)
     - **BS** = Balance Sheet (Asset, Liability, and Equity accounts)
     - **RE** = Retained Earnings
   - **Description** - Optional explanation of what this account tracks.
   - **Comment** - Optional notes about this account.

4. Click **Save**.
5. If there are any errors (duplicate number, duplicate name, missing required fields), the system will show the error message in red. Correct the issue and try again.

#### 3.3.3 Editing an Account

1. Navigate to **Chart of Accounts**.
2. Click the **Edit** link next to the account you want to modify.
3. Make your changes on the Edit page.
4. Click **Save**.

The system will reject changes that create duplicate account numbers or names.

#### 3.3.4 Deactivating an Account

Accounts cannot be permanently deleted. Instead, they are deactivated:

1. Navigate to **Chart of Accounts**.
2. Click **Edit** next to the account.
3. Click the **Deactivate** button.

**Important:** An account can only be deactivated if its **balance is zero**. If the balance is greater than zero, you will see an error message. You must first create journal entries to bring the balance to zero before the account can be deactivated.

To reactivate a deactivated account, click the **Activate** button on the Edit page.

#### 3.3.5 Searching for an Account

1. On the Chart of Accounts page, locate the **Search** field.
2. Type an account number (e.g., "1000") or part of an account name (e.g., "Cash").
3. Press Enter or click the search button.
4. The list will update to show only matching accounts.

#### 3.3.6 Filtering the Account List

The Chart of Accounts page offers several filters:

- **Category** dropdown - Filter by Asset, Liability, Equity, Revenue, or Expense
- **Subcategory** field - Filter by subcategory text
- **Show Inactive** checkbox - Check this to include deactivated accounts in the list

### 3.4 Viewing the Event Log

The Event Log is an automatic record of every change made in the system. It shows what was changed, who changed it, when, and what the data looked like before and after the change.

1. Click **Admin** in the navigation bar, then click **Event Logs**.
2. The Event Logs page shows a paginated list (25 entries per page) with columns: ID, Table, Record ID, Action, User, and Timestamp.
3. You can filter by:
   - **Table Name** - Which part of the system was changed (e.g., "ChartAccounts," "JournalEntries")
   - **Action** - What type of change (Created, Updated, Approved, Rejected, Posted, etc.)
   - **Date Range** - Filter by when the change occurred
   - **Search** - Search within the change data
4. Click on an entry to view the full details, including the **Before** and **After** JSON snapshots showing exactly what data changed.

You can also view chart-of-accounts-specific logs by navigating to **Chart of Accounts > Logs**.

### 3.5 Viewing Reports

As an Administrator, you have access to all areas of the system including the Chart of Accounts, Journal Entries, Ledger, Event Logs, Email Outbox, and all user management features. Financial reports (Trial Balance, Income Statement, Balance Sheet, Retained Earnings) are accessible through the reports section when implemented.

---

## 4. Manager Guide

### 4.1 Manager Dashboard

When you log in as a Manager, you will see the Dashboard page with:

- A **warning alert** if there are journal entries waiting for your approval (e.g., "You have 3 journal entries waiting for approval")
- An **info alert** if there are approved entries that have not been posted to the ledger yet
- A **notification** showing the count of rejected entries
- Quick-access cards to Chart of Accounts, Journal, and Ledger

Your primary responsibility is to **review and approve or reject journal entries** and **post approved entries to the ledger**.

### 4.2 Reviewing Journal Entries

#### 4.2.1 Understanding Journal Entries

A journal entry is a record of a financial transaction. Each entry has:
- A **date** when the transaction occurred
- A **description** of what happened
- One or more **lines**, each showing an account, a debit amount, or a credit amount
- The total debits must always equal the total credits (this is the fundamental rule of double-entry accounting)

#### 4.2.2 Viewing Pending Journal Entries

1. Click **Journal** in the navigation bar.
2. From the **Status** dropdown, select **Pending**.
3. The page will show all journal entries waiting for your review.

#### 4.2.3 Approving a Journal Entry

1. On the Journal list page, click on a pending entry to view its details.
2. Click the **Approve** link (or navigate directly to the Approve page).
3. Review the entry carefully:
   - Check the date and description
   - Verify all account lines are correct
   - Confirm total debits equal total credits (shown at the bottom)
4. If everything looks correct, click the **Approve** button.
5. The entry status will change to **Approved**.

**What happens when you approve:** The entry is now eligible to be posted to the ledger. It does not affect account balances until it is posted.

#### 4.2.4 Rejecting a Journal Entry

1. View the pending entry's details on the Approve page.
2. If the entry has errors or issues, you must reject it.
3. In the **Rejection Comment** text area, type a clear explanation of why the entry is being rejected (this is required - you cannot reject without a comment).
4. Click the **Reject** button.
5. The entry status will change to **Rejected**, and the accountant will be able to see your comment.

#### 4.2.5 Filtering Journal Entries by Status or Date

1. On the Journal list page, use the available filters:
   - **Status** dropdown: Pending, Approved, Rejected, Posted
   - **Date From** and **Date To**: Filter entries within a date range
2. Press Enter or click the filter button to apply.

#### 4.2.6 Searching for a Specific Journal Entry

Use the **Search** field on the Journal list page. You can search by:
- Date
- Amount (debit or credit value)
- Account name

### 4.3 Viewing the Ledger

#### 4.3.1 What is a Ledger?

A ledger is a detailed history of all transactions for a single account. It shows every time money was added to or removed from that account, along with a running balance. Think of it like a bank statement - it tracks every transaction in order.

#### 4.3.2 Navigating to an Account's Ledger

1. Go to the **Chart of Accounts** page.
2. Click on an **account name** (it is a clickable link).
3. The system will open the Ledger page for that account, showing all posted transactions.

#### 4.3.3 Reading the Ledger

The ledger table shows the following columns:

- **Date** - When the original journal entry was dated
- **Description** - What the transaction was for
- **Debit** - Amount debited to this account (if any)
- **Credit** - Amount credited to this account (if any)
- **Balance** - The running account balance after this transaction

You can filter the ledger by date range or search by amount.

#### 4.3.4 Using the Post Reference (PR)

Each ledger entry includes a **Post Reference** number (sometimes shown as "PR" or a journal entry ID). Clicking this number takes you directly to the **original journal entry** that created this ledger transaction. This is useful for investigating the details of any transaction.

### 4.4 Generating Financial Reports

#### 4.4.1 Trial Balance

A Trial Balance lists every account that has a non-zero balance, showing each account's debit or credit balance. The total of all debits must equal the total of all credits. This report is used to verify the books are in balance before preparing other financial statements.

**Note:** The financial reporting module is currently under active development as part of Sprint 4. Detailed step-by-step instructions will be added upon completion of this module.

#### 4.4.2 Income Statement

The Income Statement (also called the Profit and Loss statement) shows the organization's revenue and expenses over a period of time. Revenue minus expenses equals **Net Income** (profit) or **Net Loss**.

**Note:** The financial reporting module is currently under active development as part of Sprint 4. Detailed step-by-step instructions will be added upon completion of this module.

#### 4.4.3 Balance Sheet

The Balance Sheet shows what the organization owns (assets), what it owes (liabilities), and the owner's share (equity) at a specific point in time. The fundamental equation is: **Assets = Liabilities + Equity**.

**Note:** The financial reporting module is currently under active development as part of Sprint 4. Detailed step-by-step instructions will be added upon completion of this module.

#### 4.4.4 Retained Earnings Statement

The Retained Earnings Statement shows how the organization's retained earnings changed during a period: **Beginning Retained Earnings + Net Income - Dividends = Ending Retained Earnings**.

**Note:** The financial reporting module is currently under active development as part of Sprint 4. Detailed step-by-step instructions will be added upon completion of this module.

#### 4.4.5 Selecting a Date Range for Reports

**Note:** The financial reporting module is currently under active development as part of Sprint 4. Each report will support date range selection to generate results for a specific period.

#### 4.4.6 Saving, Printing, or Emailing a Report

**Note:** Save, print, and email functionality for financial reports is scheduled for Sprint 4 completion. Instructions will be added here once these features are available.

---

## 5. Accountant Guide

### 5.1 Accountant Dashboard

When you log in as an Accountant, you will see the Dashboard page showing:

- Notifications about rejected journal entries (entries that were returned by a Manager with comments about what needs to be fixed)
- Quick-access cards to Chart of Accounts, Journal, and Ledger

### 5.2 Creating a Journal Entry

#### 5.2.1 What is a Journal Entry?

A journal entry records a financial transaction in the accounting system. Every journal entry has two sides:

- **Debit** lines - money coming into or being charged to an account
- **Credit** lines - money going out of or being credited to an account

The most important rule is: **Total debits must equal total credits.** This is the foundation of double-entry accounting. For example, if you receive $500 in cash for a service, you would:
- **Debit** Cash for $500 (cash increases)
- **Credit** Service Revenue for $500 (revenue increases)

Both sides equal $500, so the entry is balanced.

#### 5.2.2 Step-by-Step: Creating a New Journal Entry

1. Click **Journal** in the navigation bar.
2. Click the **New Journal Entry** button (or navigate to Journal > Create).
3. **Enter the transaction date** using the date picker. This should be the date the transaction occurred.
4. **Enter a description** of the transaction (optional but recommended, e.g., "Received cash payment for consulting services").
5. The form starts with **2 blank lines**. For your first line:
   a. Select the **account** from the dropdown (e.g., "1000 - Cash").
   b. Enter the **debit amount** (e.g., 500.00) OR the **credit amount** - not both.
   c. Optionally enter a **memo** for this line.
6. For the second line:
   a. Select the **account** (e.g., "4000 - Service Revenue").
   b. Enter the **credit amount** (e.g., 500.00).
7. To **add more lines**, click the **Add Line** button.
8. To **remove a line**, click the Remove button next to that line.
9. **Verify** that your total debits equal your total credits.
10. Click **Save** to submit the entry.

The entry will be saved with a **Pending** status and sent to a Manager for review.

**Important notes:**
- The system will not let you save if debits do not equal credits.
- You must have at least one debit line and at least one credit line.
- A single line cannot have both a debit and a credit amount.
- All accounts you reference must be active (not deactivated).
- Once saved, the entry cannot be edited or deleted.

#### 5.2.3 Attaching Source Documents

You can attach supporting documents (receipts, invoices, contracts) to a journal entry:

1. After creating and saving the journal entry, you will be on the **Details** page.
2. Scroll down to the **Attachments** section.
3. Click **Choose File** and select the document from your computer.
4. Click **Upload**.

**Accepted file types:**
- PDF documents (.pdf)
- Word documents (.doc, .docx)
- Excel spreadsheets (.xls, .xlsx)
- CSV files (.csv)
- Images (.jpg, .jpeg, .png)

**Maximum file size:** 25 MB per file.

#### 5.2.4 Canceling or Resetting Before Submission

If you have started creating a journal entry but have not clicked Save yet, you can simply navigate away from the page - no data will be saved. Click any link in the navigation bar to leave the Create page without saving.

### 5.3 Tracking Your Journal Entries

#### 5.3.1 Viewing Entry Status (Pending / Approved / Rejected / Posted)

1. Click **Journal** in the navigation bar.
2. The list shows all journal entries with their current status:
   - **Pending** - Submitted and waiting for Manager review
   - **Approved** - Manager approved the entry; waiting to be posted
   - **Rejected** - Manager returned the entry with comments
   - **Posted** - Entry has been posted to the general ledger (final)

#### 5.3.2 Understanding a Rejection

If a Manager rejects your journal entry:

1. The entry will show a **Rejected** status on the Journal list.
2. Click on the entry to view its details.
3. You will see the **Manager's Comment** explaining why it was rejected.
4. Read the comment carefully to understand what needs to be corrected.
5. Create a **new** journal entry with the corrections applied. (Rejected entries cannot be edited - you must create a fresh entry.)

#### 5.3.3 Filtering and Searching Journal Entries

On the Journal list page:
- Use the **Status** dropdown to filter by Pending, Approved, Rejected, or Posted.
- Use the **Date From** and **Date To** fields to filter by entry date.
- Use the **Search** field to find entries by date, amount, or account name.

### 5.4 Viewing the Ledger

As an Accountant, you can view the ledger for any account:

1. Navigate to the **Chart of Accounts** page.
2. Click on the **account name** you want to inspect.
3. The Ledger page opens, showing all posted transactions for that account.
4. Each row shows: Date, Description, Debit, Credit, and running Balance.
5. Click the **Post Reference (PR)** number on any row to see the original journal entry that created that transaction.
6. Use the date range filters to narrow down the view to a specific period.
7. Use the search field to find transactions by amount.

---

## 6. Dashboard and Financial Ratios

### 6.1 The Dashboard

The Dashboard is the first page you see after logging in. It appears at both the Home page (/) and the dedicated Dashboard page (/Dashboard). It provides:

- **Notifications** - Alerts about items that need attention (pending approvals, rejected entries, approved entries awaiting posting)
- **Module Navigation** - Quick-access cards to Chart of Accounts, Journal, Ledger, and admin functions
- **Account Management** - Links to change your password, manage your account, or log out

The Dashboard shows different information depending on your role:
- **Managers** see alerts about entries pending their approval and entries they have approved but not yet posted.
- **All users** see counts of rejected entries.
- **Administrators** see additional cards for Access Requests, Expired Passwords, Suspend User, Event Logs, and Email Outbox.

### 6.2 Understanding Financial Ratios

Financial ratios are calculations that measure the financial health of the organization. They take numbers from the financial statements (balance sheet and income statement) and compare them to produce meaningful indicators.

**Note:** The ratio analysis dashboard is scheduled for completion in Sprint 5. A full description of each financial ratio, its formula, and its interpretation will be added to this section upon completion. The dashboard will display ratios with color-coded health indicators (green, yellow, red) based on standard accounting benchmarks.

### 6.3 Color-Coded Indicators

The dashboard uses three colors to quickly communicate the health of each financial ratio:

- **Green** - The ratio is within the healthy, normal range. No action is needed. The organization's finances look good on this measure.
- **Yellow** - The ratio is borderline or approaching the warning threshold. This is not yet critical but should be monitored closely. Consider reviewing the underlying accounts.
- **Red** - The ratio is outside the acceptable range. This may indicate a financial problem that needs immediate attention. Review the related accounts and transactions to understand the cause.

### 6.4 Financial Ratios Reference

The following ratios are planned for the dashboard. The ratio analysis feature is scheduled for Sprint 5 implementation. These descriptions use standard accounting benchmarks; the actual thresholds may be adjusted during implementation.

**Current Ratio**
- What it measures: Whether the organization has enough short-term assets to cover its short-term debts. A higher number means more ability to pay bills.
- Formula: Current Assets / Current Liabilities
- Healthy (Green): 2.0 or above - the organization has plenty of short-term resources
- Warning (Yellow): 1.0 to 1.99 - the organization can cover its debts, but there is limited cushion
- Needs Attention (Red): Below 1.0 - the organization may not have enough to cover short-term obligations

**Quick Ratio**
- What it measures: Similar to the current ratio, but excludes inventory (which may take time to sell). This gives a stricter view of short-term financial health.
- Formula: (Current Assets - Inventory) / Current Liabilities
- Healthy (Green): 1.0 or above
- Warning (Yellow): 0.5 to 0.99
- Needs Attention (Red): Below 0.5

**Debt-to-Equity Ratio**
- What it measures: How much of the organization is funded by borrowing (debt) versus owner investment (equity). Lower is generally better.
- Formula: Total Liabilities / Total Equity
- Healthy (Green): Below 1.0 - more equity than debt
- Warning (Yellow): 1.0 to 2.0 - moderate leverage
- Needs Attention (Red): Above 2.0 - heavily reliant on debt

**Return on Assets**
- What it measures: How well the organization uses everything it owns to generate profit. Higher means more efficient use of assets.
- Formula: Net Income / Total Assets
- Healthy (Green): Above 5%
- Warning (Yellow): 2% to 5%
- Needs Attention (Red): Below 2%

**Gross Profit Margin**
- What it measures: The percentage of revenue remaining after subtracting the direct costs of goods or services sold. Higher means better core profitability.
- Formula: Gross Profit / Net Revenue
- Healthy (Green): Above 40%
- Warning (Yellow): 20% to 40%
- Needs Attention (Red): Below 20%

**Net Profit Margin**
- What it measures: The percentage of revenue that becomes actual profit after all expenses. This is the bottom-line profitability indicator.
- Formula: Net Income / Net Revenue
- Healthy (Green): Above 10%
- Warning (Yellow): 5% to 10%
- Needs Attention (Red): Below 5%

**Return on Equity**
- What it measures: How much profit is generated for each dollar of owner investment. Higher means the owners' money is being used more effectively.
- Formula: Net Income / Total Equity
- Healthy (Green): Above 15%
- Warning (Yellow): 8% to 15%
- Needs Attention (Red): Below 8%

Note: These thresholds use standard accounting benchmarks. The exact values implemented in the application may be adjusted based on course requirements during Sprint 5 development.

### 6.5 Notifications Section

The notification section at the top of the Dashboard shows:

- **For Managers:**
  - Warning alert: "You have X journal entries waiting for approval" (with a link to view them)
  - Info alert: "There are X approved entries not posted yet" (with a link to view them)
- **For All Users:**
  - Count of rejected journal entries (with a link to view them)
- **When everything is clear:**
  - "No alerts right now" message

---

## 7. Troubleshooting

| Problem | Possible Cause | Solution |
|---------|----------------|----------|
| Cannot log in | Incorrect username or password | Double-check your username (it is case-sensitive) and password. Make sure Caps Lock is off. |
| Cannot log in - "account is deactivated" | Your account has been deactivated by an Administrator | Contact your Administrator to reactivate your account. |
| Cannot log in - "account is suspended" | Your account is temporarily suspended | Wait until the suspension end date, or contact your Administrator to remove the suspension early. |
| Cannot log in - "password expired" | Your password is older than 90 days | Use the **Forgot your password?** link to reset your password, or contact your Administrator. |
| Account is locked | You entered the wrong password 3 or more times | Wait 30 minutes for the automatic unlock, or contact your Administrator to unlock it immediately. |
| Cannot submit journal entry | Debits do not equal credits | Review all debit and credit amounts on your journal entry. The total debits must exactly equal the total credits. |
| Cannot submit journal entry - "inactive account" | One of the accounts you selected has been deactivated | Choose a different, active account from the dropdown. Contact your Administrator if you need the account reactivated. |
| Cannot deactivate an account | Account has a non-zero balance | The account's balance must be exactly zero before it can be deactivated. Create journal entries to move the remaining balance to another account first. |
| Cannot create a new account - "duplicate number" | Another account already uses that number | Choose a different account number that is not already in use. |
| Cannot create a new account - "duplicate name" | Another account already uses that name | Choose a different account name. |
| Error message shown in red | A validation rule was not met | Read the red error message carefully - it explains exactly what needs to be corrected. Fix the indicated issue and try again. |
| Cannot attach a document to journal entry | File type not accepted or file too large | Only these file types are accepted: PDF, DOC, DOCX, XLS, XLSX, CSV, JPG, JPEG, PNG. Maximum file size is 25 MB. |
| Page shows "Access Denied" or redirects to login | Your role does not have permission for that page | You are trying to access a page restricted to a different role. Contact your Administrator if you believe you should have access. |
| Did not receive password reset email | Email is stored in the system outbox, not sent externally | Contact your Administrator to look up the reset link in the Email Outbox. In the current system, emails are stored in the database rather than sent via external email. |
| Password rejected when changing | New password does not meet requirements | Ensure your password is at least 8 characters, starts with a letter, contains a digit and a special character, and is not one of your last 5 passwords. |
| Changes to chart of accounts not appearing | Page needs to be refreshed | Click the Chart of Accounts link again or refresh the page in your browser. |
| [FILL IN: Other issues discovered during testing] | | |

---

## 8. Glossary

| Term | Plain-Language Definition |
|------|--------------------------|
| Account | A record that tracks a specific type of money coming in, going out, or held by the organization. Examples: Cash, Accounts Payable, Sales Revenue. |
| Chart of Accounts | The complete master list of all accounts used by the organization, each with a unique number and name. |
| Debit | An entry that increases asset and expense accounts, and decreases liability, equity, and revenue accounts. Think of it as the "left side" of a transaction. |
| Credit | An entry that increases liability, equity, and revenue accounts, and decreases asset and expense accounts. Think of it as the "right side" of a transaction. |
| Journal Entry | A record of a financial transaction showing which accounts are affected and by how much. Every entry must have balanced debits and credits. |
| Ledger | A detailed chronological record of all transactions for one specific account, showing a running balance after each transaction. |
| Trial Balance | A report listing all accounts with non-zero balances, used to verify that total debits across the entire system equal total credits. |
| Income Statement | A financial report showing the organization's revenue (money earned) and expenses (money spent) over a period of time, resulting in net income or net loss. |
| Balance Sheet | A financial report showing what the organization owns (assets), what it owes (liabilities), and the owner's share (equity) at a specific date. Assets must equal liabilities plus equity. |
| Retained Earnings | The total accumulated profit that the organization has kept in the business rather than distributing to owners. |
| Posting | The process of recording an approved journal entry's effects on the individual account ledgers. Once posted, account balances are updated. |
| Adjusting Entry | A special journal entry made at the end of an accounting period to update account balances for items like prepaid expenses or accrued revenue. |
| Post Reference (PR) | A clickable link in the ledger that takes you directly back to the original journal entry that created that ledger transaction. |
| Normal Side | Whether an account normally increases with a Debit or a Credit. Assets and Expenses are debit-normal; Liabilities, Equity, and Revenue are credit-normal. |
| Pending | A journal entry that has been created and submitted but has not yet been reviewed by a Manager. |
| Approved | A journal entry that a Manager has accepted as correct. It is now ready to be posted to the ledger. |
| Rejected | A journal entry that a Manager has declined, with a written comment explaining why. The accountant can view the reason and create a corrected new entry. |
| Posted | A journal entry that has been processed and its effects recorded in the account ledgers. This is the final state - posted entries cannot be undone. |
| Event Log | An automatic record of every change made in the system, showing who changed what, when, and what the data looked like before and after the change. |
| Access Request | A form submitted by a prospective new user asking for an account. An Administrator reviews and approves or rejects the request. |
| User Roles | The three levels of access in the system: Administrator (full access), Manager (approval and reporting), and Accountant (data entry and viewing). |
| Active / Inactive | Whether an account or user is currently in use (active) or has been turned off (inactive/deactivated). |
| Suspension | A temporary block on a user account that prevents login between a start date and end date, after which access is automatically restored. |
| Password Expiration | A security feature that requires users to change their password every 90 days to maintain account security. |

---

*End of User Manual*
