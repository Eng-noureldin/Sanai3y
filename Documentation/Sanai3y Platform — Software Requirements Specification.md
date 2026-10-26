# Sanai3y Platform — Software Requirements Specification

Oct 9, 2026&#32;

> **Scope note (Oct 9, 2026):** the team will build a **simplified MVP in one month**. The binding scope for the build is the *Project Overview & Use Cases* file, together with the ERD, Class Design and Project Plan. This SRS stays as the full reference design; the parts not built this month are listed there under Future work.

## Document control

| Item | Value |
| --- | --- |
| Product | Sanai3y — web platform connecting customers with verified tradesmen |
| Version | 0.3 (Draft, revised from 0.2) |
| Context | DEPI graduation project — Full Stack .NET track |
| Market | Egypt |
| Stack | ASP.NET Core (.NET 10) · SQL Server · React.js + TypeScript |

**How to read this document**

- Every requirement has a unique ID (e.g. `REQ-05`) so it can be traced to tasks, tests and screens.
- Requirements use **shall** for mandatory behaviour and **may** for optional behaviour.
- Each requirement has a priority: **Must** (needed for the MVP demo), **Should** (in the MVP if time allows), **Could** (nice to have, can move to a later release).
- The project's goal is a graduation demo, not a commercial launch. Wherever v0.2 left a choice open, v0.3 picks the technically cleaner option; Section 12 lists every decision.
- Configurable numbers (limits, timeouts, durations) are listed once in Section 7 and referenced by name.

**Change log**

| Version | Change |
| --- | --- |
| 0.3 | Added state transition tables, priorities, settings table, missing entities and fields, acceptance rules; clarified ambiguous requirements; resolved all open questions (Section 12) |
| 0.2 | Initial draft by the team |

## 1. Introduction

### 1.1 Purpose

This document specifies the functional and non-functional requirements of Sanai3y, a web platform that connects customers with independent, verified tradesmen (plumbers, electricians, carpenters, AC technicians, painters, etc.) in the same area. It covers the full job cycle: post a problem → chat → receive quotes → agree → execute → pay → review.

The intended readers are the development team, the DEPI project supervisor and anyone testing the system.

### 1.2 Problem statement

Residents of Egypt's new cities struggle to find a trustworthy tradesman at a fair price. They have no established word-of-mouth network, and few workshops are nearby. Tradesmen, in turn, have no simple way to reach customers in these areas or to build a public track record.

### 1.3 Goals

- Let a customer get comparable quotes from several verified tradesmen for one problem.
- Build trust through mandatory identity verification and reviews based only on real, completed jobs.
- Keep the first release cheap to run: no payment gateway, no SMS, no commission.

### 1.4 Scope

**In scope (MVP):** registration and login, mandatory worker verification, worker profiles, search, open and direct requests, multi-worker chat, quotations, cash payment confirmation, mutual reviews, basic disputes, in-app and email notifications, basic admin panel.

**Out of scope (later releases):** online payment and escrow, commissions, subscriptions, native mobile app, push notifications, urgent requests, advertising, SMS, criminal-record check (built but disabled in the MVP).

### 1.5 Glossary

| Term | Meaning |
| --- | --- |
| Customer | A registered user who needs a service |
| Worker | A registered tradesman who provides a service; must be verified before working |
| Admin | A platform administrator |
| Guest | A visitor who is not logged in |
| Profession | A trade category, e.g. Plumbing, Electrical |
| City / District | Two-level location hierarchy; a district belongs to one city |
| Service area | A district in which a worker accepts jobs |
| Verification | Admin review of a worker's identity documents before the worker can be found or send quotes |
| Job request | A service request created by a customer for one profession in one district |
| Open request | A request visible to all matching verified workers |
| Direct request | A request addressed to one specific worker |
| Quotation (quote) | A price offer from a worker for a request |
| Conversation | A chat between one customer and one worker about one request |
| Completion | The state reached when the job is confirmed done (Section 3.1) |
| Dispute | A formal complaint about a job, handled by an admin |
| Report | A flag raised by a user against a message, review or user |
| OTP | One-time password used to verify an email address or reset a password |

## 2. Overall description

### 2.1 Actors

| Actor | What they can do |
| --- | --- |
| Guest | Browse cities, professions and verified worker profiles; must register to contact anyone |
| Customer | Create requests, browse workers, chat with several workers, accept one quote, confirm completion and payment, review, report, open disputes |
| Worker (pending) | Complete profile, upload verification documents, see verification status; cannot be found, see requests, chat or quote |
| Worker (approved) | Manage profile, availability and service areas; see matching open requests; receive direct requests; chat; send quotes; execute and confirm jobs; review customers |
| Admin | Review verification, manage cities, districts and professions, manage users, resolve disputes and reports, hide reviews, view reports and audit log |
| System | Background jobs: expire requests, auto-complete jobs, send batched notifications |

A user has exactly one role. A person who is both a customer and a worker uses two accounts with two different emails.

### 2.2 Main scenario — open request

1. The customer selects a profession, city and district, describes the problem and attaches up to 5 photos.
2. The system publishes the request to verified workers whose profession and service area match.
3. An interested worker opens a conversation with the customer, sends a quote, or both.
4. The customer chats with several workers at the same time and compares their quotes side by side.
5. The customer accepts exactly one quote. The request becomes **Agreed**; all other quotes are closed and their workers are notified.
6. The worker marks the job as started (**InProgress**), performs it, then marks it done and enters the amount received.
7. The customer confirms completion and the amount paid. The request becomes **Completed**.
8. Each party may review the other.

### 2.3 Alternative scenario — direct request

1. The customer browses verified workers in a city and filters by rating, completed jobs and experience.
2. The customer opens a worker's profile and taps "Request this worker", filling the same form as an open request.
3. The system creates a **Direct** request visible only to that worker, plus a conversation between the two.
4. The worker sends a quote or declines. If the worker declines, or does not respond within `DirectRequestResponseHours`, the customer is notified and may convert the request to an open request with one click.
5. From acceptance onwards the flow is identical to the open request scenario.

### 2.4 Exception scenarios

- **No worker responds:** an open request expires after `OpenRequestExpiryDays`; the customer is notified and may re-post it.
- **Cancellation:** see REQ-06 and the request state machine (Section 3.1).
- **Disagreement on the job or the amount:** either party opens a dispute (Section 4.10).
- **Worker blocked during an active job:** the request moves to **Cancelled** with reason "Worker suspended" and the customer is notified.

### 2.5 Assumptions and constraints

- Market is Egypt: currency EGP, Arabic RTL interface first, Egyptian mobile numbers (+20).
- No commission in the first release, but the payment design shall support one later.
- Fixed stack: .NET 10, SQL Server, React.js.
- OTP and notifications are sent by email in the MVP to avoid SMS cost. The phone number is collected but **not verified** in the MVP.
- Unverified workers are hidden from search and cannot receive requests or send quotes.
- The system is built and demonstrated with seed data: one city, four districts and five professions (Section 12).
- All national-ID images and numbers used in development and the demo are fake test data.

## 3. Lifecycles and state machines

Every state change shall happen only through the transitions listed here; any other change is rejected by the domain layer and covered by unit tests.

### 3.1 Job request

**States:** `Open`, `Agreed`, `InProgress`, `PendingConfirmation`, `Completed`, `Expired`, `Cancelled`, `Disputed`. Terminal states: `Completed`, `Expired`, `Cancelled`.


&#91;embedded content: job request state machine · 8 states, transitions T1–T13\]

The main path runs down the left; Disputed is the only state an admin resolves, and it always ends in Completed or Cancelled.

| # | From | To | Triggered by | Condition and effect |
| --- | --- | --- | --- | --- |
| T1 | — | Open | Customer | Submits the form. Customer has fewer than `MaxActiveOpenRequests` active requests. Matching workers are notified (open) or the chosen worker is notified (direct). |
| T2 | Open | Agreed | Customer | Accepts one quote. In one transaction: quote → Accepted (snapshot saved), all other quotes → Closed, request stores `AcceptedQuotationId`. RowVersion check prevents a double accept. |
| T3 | Open | Expired | System | `ExpiresAt` reached with no accepted quote. Customer and quoting workers are notified. |
| T4 | Open | Cancelled | Customer | Any time, reason optional. Pending quotes → Closed, workers notified. |
| T5 | Agreed | InProgress | Worker | Taps "Start job". Only the assigned worker can start it. |
| T6 | Agreed | Cancelled | Customer or worker | Reason mandatory. Counted in the user's cancellation stats. Customer may re-post the request as a new one. |
| T7 | InProgress | PendingConfirmation | Worker | Taps "Job done" and enters the amount received. Sets `WorkerConfirmedAt`. |
| T8 | PendingConfirmation | Completed | Customer | Confirms and enters the amount paid. Amounts match → Completed; worker's completed-jobs count +1. |
| T9 | PendingConfirmation | Completed | System | `AutoCompleteHours` (default 72) passed since T7 with no customer action. Amount = worker's amount. |
| T10 | PendingConfirmation | Disputed | Customer or system | Customer objects, or the two amounts differ (PAY-02). A dispute is created automatically. |
| T11 | InProgress | Cancelled | Customer or worker | Reason mandatory. Disagreements about money go through a dispute instead. |
| T12 | InProgress or Completed | Disputed | Customer or worker | Opens a dispute (DSP-01). From Completed only within `DisputeWindowDays`. |
| T13 | Disputed | Completed or Cancelled | Admin | Final state is part of the dispute decision. Completed-jobs count and rating are recalculated. |

### 3.2 Quotation

**States:** `Pending`, `RevisionRequested`, `Accepted`, `Rejected`, `Withdrawn`, `Closed`.

| From | To | Triggered by | Condition and effect |
| --- | --- | --- | --- |
| — | Pending | Worker | Worker is approved, request is Open, worker has no other active quote on this request (QTE-03). |
| Pending | RevisionRequested | Customer | Optional note explaining what to change. |
| Pending or RevisionRequested | Pending | Worker | Edits the quote. Version number +1; previous versions are kept for dispute review. |
| Pending | Accepted | Customer | See request transition T2. An immutable snapshot is stored. |
| Pending or RevisionRequested | Rejected | Customer | Worker is notified and may send one new quote. |
| Pending or RevisionRequested | Withdrawn | Worker | Before acceptance only. |
| Pending or RevisionRequested | Closed | System | Another quote was accepted, or the request expired or was cancelled. |

**Price change after acceptance (QTE-06):** the worker sends an *amendment* linked to the accepted quote. If the customer approves it, a new snapshot replaces the active one and the old one is kept. Allowed only while the request is Agreed or InProgress.

### 3.3 Worker verification

**States:** `NotSubmitted`, `Pending`, `UnderReview`, `Approved`, `Rejected`, `Revoked`.

| From | To | Triggered by | Condition and effect |
| --- | --- | --- | --- |
| NotSubmitted | Pending | Worker | All required documents uploaded and ID number entered. |
| Pending | UnderReview | Admin | Admin opens the submission; it is locked to that admin. |
| UnderReview | Approved | Admin | Worker notified; "Verified" badge shown; worker becomes searchable. |
| UnderReview | Rejected | Admin | Rejection reason is mandatory and shown to the worker. |
| Rejected | Pending | Worker | Re-submits corrected documents. Max `VerificationMaxAttempts`, then contact support. |
| Approved | Revoked | Admin | Reason mandatory, e.g. fraud found. Worker hidden immediately; active requests follow the "worker blocked" exception (2.4). |

### 3.4 Dispute

**States:** `Open`, `UnderReview`, `Resolved`, `Withdrawn`.

| From | To | Triggered by | Condition and effect |
| --- | --- | --- | --- |
| — | Open | Customer, worker or system | Request moves to Disputed. Other party is notified and may add their side and evidence. |
| Open | UnderReview | Admin | Admin takes the case. |
| UnderReview | Resolved | Admin | Decision, outcome(s) and final request state recorded (DSP-04). Both parties notified. |
| Open | Withdrawn | Opener | Request returns to the state it had before the dispute. |

## 4. Functional requirements

Names in backticks such as `OtpExpiryMinutes` are configurable settings defined in Section 7.

### 4.1 Identity and access (AUTH)

| ID | Requirement | Priority |
| --- | --- | --- |
| AUTH-01 | A guest shall register as Customer or Worker with full name, email, Egyptian mobile number, password and role. The mobile number shall match `01[0125]` followed by 8 digits and is stored in E.164 form (+20…). | Must |
| AUTH-02 | Email and mobile number shall each be unique. The account stays inactive until the email is verified with an OTP. The mobile number is not verified in the MVP. | Must |
| AUTH-03 | The OTP shall be 6 digits, valid for `OtpExpiryMinutes`, single use, and invalid after `OtpMaxAttempts` wrong tries. Resend is allowed after `OtpResendCooldownSeconds`. | Must |
| AUTH-04 | A user shall log in with email or mobile number plus password. | Must |
| AUTH-05 | Login shall return a JWT access token valid for `AccessTokenMinutes` and a refresh token valid for `RefreshTokenDays`. Refresh tokens are stored hashed, rotated on every use and revocable. | Must |
| AUTH-06 | The system shall enforce role-based access with three roles: Customer, Worker, Admin. Admin accounts are created by seeding or by another admin, never by public registration. | Must |
| AUTH-07 | Passwords shall be at least 8 characters with at least one letter and one digit. | Must |
| AUTH-08 | A user shall reset a forgotten password with an email OTP. All refresh tokens are revoked after a reset. | Must |
| AUTH-09 | An admin shall block a user. All refresh tokens are revoked and every API call checks the user's status, so the block takes effect on the next request even with a valid access token. | Must |
| AUTH-10 | After `LoginMaxFailedAttempts` failed logins the account is locked for `LoginLockoutMinutes`. | Must |
| AUTH-11 | Registration shall require accepting the terms and privacy policy; the accepted version and time are stored. | Must |
| AUTH-12 | A logged-in user shall change their password and log out (revokes the current refresh token). | Should |
| AUTH-13 | A user shall request account deletion (see 6.6). | Should |

### 4.2 Worker verification (VER)

| ID | Requirement | Priority |
| --- | --- | --- |
| VER-01 | A worker shall upload the national ID front, back and a selfie holding the ID, and enter the 14-digit national ID number. | Must |
| VER-02 | Criminal-record certificate upload is built but hidden while `CriminalRecordRequired` is false (default in the MVP). | Could |
| VER-03 | Verification follows the state machine in 3.3. | Must |
| VER-04 | An admin shall review submissions from a queue (oldest first), view the documents, and approve or reject with a mandatory reason. The worker is notified in-app and by email. | Must |
| VER-05 | A worker who is not Approved shall not appear in search, see open requests, receive direct requests, chat or send quotes. This is enforced on the server, not only hidden in the UI. | Must |
| VER-06 | A pending worker shall be able to complete their profile (photo, bio, professions, service areas, portfolio) while waiting. | Must |
| VER-07 | Verification documents shall be encrypted at rest in private storage and served only through an admin-only endpoint. Every view is written to the audit log. | Must |
| VER-08 | A national ID number shall not be used by two accounts. Uniqueness is checked with a keyed hash (HMAC) of the number, because the encrypted value itself cannot be compared or indexed. | Must |
| VER-09 | A "Verified" badge shall appear on approved profiles and worker cards. | Must |
| VER-10 | The system shall validate the ID number format (14 digits, valid birth-date part) and reject applicants under 18. | Could |

### 4.3 Profiles (PRO)

| ID | Requirement | Priority |
| --- | --- | --- |
| PRO-01 | A worker profile shall have name, photo, bio (max 500 characters) and years of experience (0–60). | Must |
| PRO-02 | A worker shall select 1 to `MaxProfessionsPerWorker` professions. | Must |
| PRO-03 | A worker shall select one city and one or more districts in it as service areas. One city per worker in the MVP. | Must |
| PRO-04 | A worker shall upload up to `MaxPortfolioPhotos` portfolio photos with optional captions. | Should |
| PRO-05 | A worker shall set availability to Available or Busy. Busy workers are hidden from search and receive no new-request notifications, but can still open the request feed and quote. | Must |
| PRO-06 | A worker may set a weekly schedule (days and hours). | Could |
| PRO-07 | The system shall compute completed jobs, average rating and rating count from real data; workers cannot edit them. | Must |
| PRO-08 | A customer profile shall have name, optional photo and city. The system shows workers the customer's completed requests, average rating as a customer and cancellation count. | Must |
| PRO-09 | A public worker profile shall list reviews, newest first, paginated. | Must |

### 4.4 Search and discovery (DSC)

| ID | Requirement | Priority |
| --- | --- | --- |
| DSC-01 | A user or guest shall select a city (required) and optionally a district to list workers who serve it. | Must |
| DSC-02 | Results shall be filterable by profession, minimum rating, availability and minimum completed jobs. | Must |
| DSC-03 | Results shall be sortable by top rated, most completed jobs and newest. | Must |
| DSC-04 | All lists shall be paginated (default `PageSizeDefault`, max `PageSizeMax`). | Must |
| DSC-05 | A worker card shall show photo, name, professions, rating and count, completed jobs, Verified badge and availability. | Must |
| DSC-06 | Search shall return only Approved, active (not blocked) workers. | Must |
| DSC-07 | Guests can browse; contacting a worker or creating a request requires login. | Must |
| DSC-08 | Workers with fewer than `MinReviewsToShowRating` reviews show "New" instead of a star average, so one 5-star review does not outrank many 4.8 reviews. | Should |

### 4.5 Job requests (REQ)

| ID | Requirement | Priority |
| --- | --- | --- |
| REQ-01 | A customer shall create a request with profession, city, district, description (20–1000 characters), up to `MaxRequestPhotos` photos and a preferred time (date plus Morning / Afternoon / Evening, or Flexible). | Must |
| REQ-02 | A request shall be Open or Direct (scenarios 2.2 and 2.3). | Must |
| REQ-03 | The customer shall enter a detailed address, stored privately and shown to the worker only after the request is Agreed. Before that, workers see only the district. | Must |
| REQ-04 | An open request shall be visible only to Approved workers who have the request's profession and the request's district in their service areas. | Must |
| REQ-05 | Several workers may chat and quote on the same open request at the same time. | Must |
| REQ-06 | Accepting a quote follows transition T2: exactly one accepted quote, all others closed and their workers notified. | Must |
| REQ-07 | Cancellation follows transitions T4, T6 and T11; the reason is mandatory after Agreed. | Must |
| REQ-08 | Open requests shall expire after `OpenRequestExpiryDays` via a background job; the customer is notified `ExpiryReminderHours` before. | Must |
| REQ-09 | A customer shall have at most `MaxActiveOpenRequests` requests in state Open at once. | Must |
| REQ-10 | Approved workers see all matching open requests, newest first, with no daily limit (`WorkerDailyRequestLimit` = 0 means unlimited). | Must |
| REQ-11 | A customer may edit the description and photos of an Open request until it receives its first quote. | Should |
| REQ-12 | Workers see the customer's first name, district, customer rating and request details only — never phone, email or address before Agreed. | Must |
| REQ-13 | Customers shall have a "My requests" list and workers a "My jobs" list, both filterable by status. | Must |

### 4.6 Chat (CHT)

| ID | Requirement | Priority |
| --- | --- | --- |
| CHT-01 | A conversation is unique per (request, customer, worker). On an open request only the worker can start it; on a direct request it is created with the request. | Must |
| CHT-02 | A customer may hold several conversations on the same request with different workers. | Must |
| CHT-03 | Messages shall be delivered in real time with SignalR. | Must |
| CHT-04 | Messages shall show sent, delivered and read status. | Should |
| CHT-05 | Attachments are images only: max `ChatMaxImagesPerMessage` per message and `ChatMaxImageMB` each. | Must |
| CHT-06 | A user can open only conversations they belong to; this is checked on every API call and every SignalR hub method. | Must |
| CHT-07 | Before Agreed, the system shall mask phone numbers (Latin and Arabic-Indic digits, with or without spaces and dashes), emails and links (incl. WhatsApp and Telegram) in delivered messages. The original text is kept for dispute review. Masking is best effort and is backed by reports. | Must |
| CHT-08 | Once a request is Agreed, conversations with the other workers become read-only. | Must |
| CHT-09 | Once a request is Agreed, both parties' mobile numbers and the address are shown on the request page, because the worker must coordinate the visit. | Must |
| CHT-10 | Chat history is kept permanently. Admins can read a conversation only when it is linked to a dispute or a report; each access is audited. | Must |
| CHT-11 | A user shall be able to report a message or a user (see 4.11). | Must |

### 4.7 Quotations (QTE)

| ID | Requirement | Priority |
| --- | --- | --- |
| QTE-01 | A quote shall contain price type (Fixed or Range), price or min/max (min < max), description, expected duration (value + hours or days) and whether materials are included. | Must |
| QTE-02 | Quotes follow the state machine in 3.2 (accept, reject, request revision, edit, withdraw). | Must |
| QTE-03 | A worker has at most one active quote per request. | Must |
| QTE-04 | The customer sees a side-by-side comparison of all active quotes: price, duration, materials, worker rating and completed jobs. | Must |
| QTE-05 | The accepted quote is stored as an immutable snapshot. | Must |
| QTE-06 | Changing the price after acceptance requires an amendment approved by the customer (3.2). | Should |
| QTE-07 | For a Range quote, the final amount shall be within the range; a higher amount requires an approved amendment before the worker marks the job done. | Should |
| QTE-08 | A quote appears as a card inside the conversation as well as in the comparison view. | Should |

### 4.8 Payments (PAY)

| ID | Requirement | Priority |
| --- | --- | --- |
| PAY-01 | In the MVP the only method is cash paid to the worker after the job. | Must |
| PAY-02 | A payment record is created at transition T7 with the worker's amount, completed with the customer's amount at T8. If the amounts differ, a dispute is opened automatically (T10). | Must |
| PAY-03 | Payments go through an `IPaymentProvider` interface (Strategy pattern); the MVP has only `CashPaymentProvider`, so Paymob, Fawry, InstaPay or wallets can be added later. | Should |
| PAY-04 | The payment record has a `PlatformFee` field, 0 in the MVP. | Must |
| PAY-05 | All amounts are EGP, stored as decimal(18,2). | Must |

### 4.9 Reviews (REV)

| ID | Requirement | Priority |
| --- | --- | --- |
| REV-01 | Each party may review the other (1–5 stars, optional comment up to 500 characters) once the request is Completed, within `ReviewWindowDays`. | Must |
| REV-02 | One review per party per request, editable for `ReviewEditHours` after submission. | Must |
| REV-03 | Average rating = mean of visible reviews, rounded to 1 decimal. Hidden reviews are excluded. | Must |
| REV-04 | The reviewed party may reply once to a review. | Should |
| REV-05 | An admin may hide an abusive review with a recorded reason; the author is notified. | Must |
| REV-06 | Reviews are published when both parties have submitted or the review window ends, so neither side can retaliate. | Could |

### 4.10 Disputes (DSP)

| ID | Requirement | Priority |
| --- | --- | --- |
| DSP-01 | Either party may open a dispute on an InProgress, PendingConfirmation or Completed request (Completed only within `DisputeWindowDays`), with description and up to 5 images. | Must |
| DSP-02 | Only one open dispute per request. | Must |
| DSP-03 | Disputes follow the state machine in 3.4. | Must |
| DSP-04 | The admin sees the full chat, all quote versions, the accepted snapshot and both payment confirmations. | Must |
| DSP-05 | The decision records the final request state plus zero or more outcomes: warning, review removal, account suspension (with end date). The platform does not move money in the MVP. | Must |

### 4.11 Reports (RPT) — new in v0.3

| ID | Requirement | Priority |
| --- | --- | --- |
| RPT-01 | A user shall report a message, a review or a user with a reason (spam, abuse, fraud, off-platform contact, other) and an optional note. | Must |
| RPT-02 | A user cannot report the same item twice. | Should |
| RPT-03 | Admins handle reports from a queue: dismiss, hide content, warn or block the user. | Must |

### 4.12 Notifications (NTF)

| ID | Requirement | Priority |
| --- | --- | --- |
| NTF-01 | In-app notifications for: new matching open request, new direct request, new message, new or edited quote, revision requested, quote accepted / rejected / closed, every request state change, verification result, dispute and report updates, new review, request about to expire. | Must |
| NTF-02 | An in-app notification centre with unread count and mark-as-read, updated in real time over SignalR. | Must |
| NTF-03 | Email copies of notifications. Chat messages are emailed only if still unread after `UnreadMessageEmailDelayMinutes`. | Must |
| NTF-04 | Users can turn each notification category on or off per channel. OTP, password and security emails cannot be turned off. | Should |
| NTF-05 | Repeated notifications are batched: new matching requests at most once per `MatchingRequestDigestMinutes`, unread messages in one email per conversation. | Should |
| NTF-06 | SMS is added later behind an `ISmsProvider` interface. | Could |

### 4.13 Admin panel (ADM)

| ID | Requirement | Priority |
| --- | --- | --- |
| ADM-01 | Verification queue with document viewer and approve / reject actions. | Must |
| ADM-02 | Manage cities, districts and professions (Arabic and English names, active flag). Items in use can be deactivated, not deleted. | Must |
| ADM-03 | Manage users: search by name, email or phone; view; block and unblock. | Must |
| ADM-04 | Queues for disputes, reports and hidden reviews. | Must |
| ADM-05 | Dashboard: user counts by role, requests by state, completion rate, distribution by city and profession, for a chosen date range. | Should |
| ADM-06 | Audit log of every admin action and every access to verification documents or chats (who, what, when, target). | Must |
| ADM-07 | Settings page to edit the values in Section 7 without redeploying. | Could |

## 5. Core business rules

These rules are enforced in the domain layer and each one has at least one unit test.

| ID | Rule |
| --- | --- |
| BR-01 | A review can be created only by a participant of a Completed request, within `ReviewWindowDays`. |
| BR-02 | A user cannot review themselves, and a worker cannot review a job they did not perform. |
| BR-03 | A worker's completed-jobs count equals the number of their requests currently in state Completed. It is recalculated if a dispute changes the final state. |
| BR-04 | At most one quote per request can be Accepted. Acceptance runs in one transaction with an optimistic-concurrency check (RowVersion) on the request. |
| BR-05 | A worker who is not Approved never sees open requests, never appears in search and cannot quote or chat. |
| BR-06 | Phone numbers, emails and the customer's address are never shown between the parties before Agreed; after Agreed they are shown (CHT-09). |
| BR-07 | A request is Completed when the customer confirms (T8), or automatically `AutoCompleteHours` after the worker marks it done if the customer neither confirms nor objects (T9). |
| BR-08 | A request has at most one open dispute at a time. |
| BR-09 | A blocked or suspended user cannot log in; their Open requests are cancelled and their active jobs follow exception 2.4. |
| BR-10 | Cities, districts and professions referenced by any record are deactivated, never hard-deleted. All main entities use soft delete. |

## 6. Non-functional requirements

Each requirement is measurable so it can be tested. Priorities reflect a graduation-project timeline: items marked Should or Could are designed for but may be demonstrated in a simpler form.

### 6.1 Security

| ID | Requirement | Priority |
| --- | --- | --- |
| SEC-01 | Passwords hashed with ASP.NET Core Identity's default hasher (PBKDF2) or Argon2id/BCrypt, salted. | Must |
| SEC-02 | HTTPS only, HSTS enabled, CORS restricted to the frontend origin. | Must |
| SEC-03 | OWASP Top 10: server-side validation (FluentValidation) on every input, EF Core parameterised queries, output encoding in React, no raw HTML rendering of user text. | Must |
| SEC-04 | Resource-based authorization on every endpoint and SignalR hub method (e.g. a worker can edit only their own quote). | Must |
| SEC-05 | File uploads: allowed types JPEG, PNG, WebP checked by magic bytes; size limits from Section 7; files renamed to random names; stored outside the database and the web root; EXIF metadata (incl. GPS) stripped. | Must |
| SEC-06 | National ID number and verification documents encrypted at rest; keys kept outside the database (user secrets or key vault, never in source control). | Must |
| SEC-07 | Rate limiting with ASP.NET Core's built-in rate limiter on OTP, login, search and messaging (limits in Section 7). | Must |
| SEC-08 | Secrets (connection strings, JWT key, email API key) never committed to Git. | Must |

### 6.2 Performance

| ID | Requirement | Priority |
| --- | --- | --- |
| PRF-01 | Worker search returns in under 500 ms (p95) with 10,000 workers and 100,000 requests in the database. | Should |
| PRF-02 | Other API calls under 800 ms at p95 under normal load. | Should |
| PRF-03 | A chat message reaches an online recipient in under 1 second. | Should |
| PRF-04 | Indexes at least on: ServiceArea (DistrictId, WorkerId); WorkerProfession (ProfessionId, WorkerId); WorkerProfile (VerificationStatus, IsAvailable); JobRequest (Status, ProfessionId, DistrictId); Quotation (JobRequestId); Message (ConversationId, SentAt). | Must |
| PRF-05 | A seed script generates the volumes in PRF-01 so the target can be tested. | Should |

> Fix vs v0.2: v0.2 proposed an index on (CityId, ProfessionId, IsVerified, IsAvailable), but no table holds all four columns. City and profession live in ServiceArea and WorkerProfession, so the indexes above follow the actual data model.

### 6.3 Scalability

| ID | Requirement | Priority |
| --- | --- | --- |
| SCL-01 | The API is stateless (no in-memory session), so it can run on several instances. | Must |
| SCL-02 | A SignalR Redis backplane is added when more than one instance runs. | Could |
| SCL-03 | Modular monolith: each module owns its tables and is called through its application layer, not by reading another module's tables. | Should |

### 6.4 Availability and monitoring

| ID | Requirement | Priority |
| --- | --- | --- |
| AVL-01 | The deployed demo runs in Docker with automatic restart on failure. | Could |
| AVL-02 | Structured logging with Serilog including a correlation ID per request. | Must |
| AVL-03 | Health-check endpoint covering database, storage and email provider. | Should |
| AVL-04 | Daily database backup, with a restore tested at least once before launch. | Should |

### 6.5 Usability and accessibility

| ID | Requirement | Priority |
| --- | --- | --- |
| USE-01 | Arabic RTL is the default; all UI text comes from translation files so English can be added (i18n). | Must |
| USE-02 | Mobile-first responsive layout, usable from 360 px width. | Must |
| USE-03 | A new customer can post a request in under 3 minutes without help (checked in a usability test with 5 people). | Should |
| USE-04 | WCAG 2.1 AA where feasible: colour contrast, keyboard navigation, form labels, alt text. | Should |
| USE-05 | Dates shown in Africa/Cairo time; stored in UTC. | Must |

### 6.6 Privacy and compliance

| ID | Requirement | Priority |
| --- | --- | --- |
| PRV-01 | The platform shall follow Egypt's Personal Data Protection Law No. 151 of 2020. The demo uses fake identity data only. | Must |
| PRV-02 | Collect only the data each feature needs; the privacy policy lists what is collected and why. | Must |
| PRV-03 | A user may request deletion of their account. Personal data is anonymised; records needed for other users or legal retention (completed jobs, reviews, disputes) are kept without identifying data. | Should |
| PRV-04 | Verification documents are deleted `VerificationDocRetentionDays` after a final decision, keeping only the ID hash and the decision. | Could |

### 6.7 Maintainability and quality

| ID | Requirement | Priority |
| --- | --- | --- |
| MNT-01 | Clean Architecture: Domain, Application, Infrastructure, API projects. | Must |
| MNT-02 | Unit tests for every state machine transition and business rule; integration tests for the main API flows (register → request → quote → accept → complete → review). | Must |
| MNT-03 | CI pipeline (e.g. GitHub Actions) builds and runs tests on every pull request; EF Core migrations applied automatically on deploy. | Should |
| MNT-04 | API documented with OpenAPI / Swagger; errors returned in a consistent ProblemDetails format. | Must |

## 7. Configurable settings

All limits live in one place (the Setting table, seeded with these defaults) so code, tests and the demo use the same numbers.

| Setting | Default | Used by |
| --- | --- | --- |
| OtpExpiryMinutes | 10 | AUTH-03 |
| OtpMaxAttempts | 5 | AUTH-03 |
| OtpResendCooldownSeconds | 60 | AUTH-03 |
| OtpRequestsPerHourPerEmail | 5 | SEC-07 |
| AccessTokenMinutes | 15 | AUTH-05 |
| RefreshTokenDays | 7 | AUTH-05 |
| LoginMaxFailedAttempts | 5 | AUTH-10 |
| LoginLockoutMinutes | 15 | AUTH-10 |
| SearchRequestsPerMinutePerIp | 60 | SEC-07 |
| MessagesPerMinutePerUser | 30 | SEC-07 |
| CriminalRecordRequired | false | VER-02 |
| VerificationMaxAttempts | 3 | 3.3 |
| VerificationDocMaxMB | 10 | SEC-05 |
| VerificationDocRetentionDays | 90 | PRV-04 |
| MaxProfessionsPerWorker | 3 | PRO-02 |
| MaxPortfolioPhotos | 20 | PRO-04 |
| ImageMaxMB | 5 | REQ-01, PRO-04 |
| PageSizeDefault / PageSizeMax | 20 / 50 | DSC-04 |
| MinReviewsToShowRating | 3 | DSC-08 |
| MaxRequestPhotos | 5 | REQ-01 |
| OpenRequestExpiryDays | 7 | REQ-08 |
| ExpiryReminderHours | 24 | REQ-08 |
| MaxActiveOpenRequests | 3 | REQ-09 |
| WorkerDailyRequestLimit | 0 (unlimited) | REQ-10 |
| DirectRequestResponseHours | 24 | 2.3 |
| ChatMaxImagesPerMessage | 5 | CHT-05 |
| ChatMaxImageMB | 5 | CHT-05 |
| AutoCompleteHours | 72 | BR-07 |
| DisputeWindowDays | 7 | DSP-01 |
| ReviewWindowDays | 14 | REV-01 |
| ReviewEditHours | 48 | REV-02 |
| UnreadMessageEmailDelayMinutes | 15 | NTF-03 |
| MatchingRequestDigestMinutes | 60 | NTF-05 |

## 8. Architecture

The system is one ASP.NET Core API (modular monolith) serving a React single-page app, with SQL Server for data, private file storage for images and documents, and Hangfire for background jobs.

| Layer | Decision |
| --- | --- |
| Backend | ASP.NET Core (.NET 10) Web API, modular monolith, CQRS-style commands and queries (MediatR or a small in-house dispatcher — check MediatR's current licence first), FluentValidation |
| Identity | ASP.NET Core Identity for users, roles and password hashing; custom JWT + refresh-token issuing |
| Database | SQL Server with EF Core Code First, soft delete via global query filters, RowVersion on JobRequest and Quotation |
| Frontend | React + TypeScript, React Router, TanStack Query (React Query), RTL-ready UI library, i18n library for translations |
| Real-time | SignalR hubs for chat and notifications, authenticated with the same JWT |
| File storage | `IFileStorage` interface: local disk in development, cloud blob storage in production. Public bucket/container for profile, portfolio and request photos; private one for verification documents and dispute evidence |
| Email | `IEmailSender` interface backed by a transactional email provider |
| Background jobs | Hangfire: request expiry and reminders, auto-completion (T9), notification digests, unread-message emails |
| Cache | Optional Redis (or in-memory cache in the MVP) for cities, districts and professions |
| API | REST, versioned (`/api/v1`), OpenAPI / Swagger, ProblemDetails errors |

**Modules:** Identity · Verification · Workers · Customers · Locations & Professions · Discovery · Requests · Quotations · Messaging · Payments · Reviews · Disputes & Reports · Notifications · Admin & Audit

> Advice for the team: the architecture above is ambitious for a beginner team. Build the Must items with a plain layered structure first; add CQRS pipelines, Redis and the SignalR backplane only once the core job flow works end to end.

## 9. Data model

The model below is the starting point for EF Core entities. All tables also have `CreatedAt`, `UpdatedAt` and, where soft-deleted, `IsDeleted`. Status fields are enums matching Section 3.

### 9.1 Identity and profiles

| Entity | Key fields | Notes |
| --- | --- | --- |
| User | Id, FullName, Email, PhoneNumber, PasswordHash, Role, Status (Active, Blocked, Suspended, Deleted), EmailConfirmed, SuspendedUntil | Based on Identity's user table |
| RefreshToken | Id, UserId, TokenHash, ExpiresAt, RevokedAt, ReplacedByTokenId | New |
| OtpCode | Id, UserId, Purpose (VerifyEmail, ResetPassword), CodeHash, ExpiresAt, Attempts, UsedAt | New |
| TermsAcceptance | UserId, TermsVersion, AcceptedAt | New |
| CustomerProfile | UserId, PhotoUrl, CityId, CompletedRequestsCount, AvgRating, RatingCount, CancellationCount | New |
| WorkerProfile | UserId, PhotoUrl, Bio, ExperienceYears, CityId, VerificationStatus, IsAvailable, CompletedJobsCount, AvgRating, RatingCount, CancellationCount | CityId added (PRO-03) |
| WorkerProfession | WorkerId, ProfessionId | Composite key |
| ServiceArea | WorkerId, DistrictId | Composite key |
| PortfolioPhoto | Id, WorkerId, Url, Caption, SortOrder | New |
| WeeklySchedule | WorkerId, DayOfWeek, From, To | Could |

### 9.2 Verification

| Entity | Key fields | Notes |
| --- | --- | --- |
| VerificationSubmission | Id, WorkerId, NationalIdEncrypted, NationalIdHash (unique), Status, SubmittedAt, ReviewedBy, ReviewedAt, RejectReason, AttemptNumber | New: groups the documents of one attempt |
| VerificationDocument | Id, SubmissionId, Type (IdFront, IdBack, Selfie, CriminalRecord), StoragePath, ContentType | Linked to a submission instead of directly to the worker |

### 9.3 Reference data

| Entity | Key fields | Notes |
| --- | --- | --- |
| City | Id, NameAr, NameEn, IsActive | Split from District |
| District | Id, CityId, NameAr, NameEn, IsActive | v0.2 used one table with ParentId; two tables are simpler to validate |
| Profession | Id, NameAr, NameEn, IconUrl, IsActive |  |
| Setting | Key, Value, Description | Section 7 values (ADM-07) |

### 9.4 Jobs

| Entity | Key fields | Notes |
| --- | --- | --- |
| JobRequest | Id, CustomerId, ProfessionId, DistrictId, Description, AddressDetails, PreferredDate, PreferredSlot, Type, TargetWorkerId (direct only), Status, ExpiresAt, AcceptedQuotationId, WorkerConfirmedAt, CompletedAt, CancelledBy, CancelReason, RowVersion | Address, preferred time, target worker and confirmation fields added |
| JobRequestPhoto | Id, JobRequestId, Url | New |
| JobRequestStatusHistory | Id, JobRequestId, FromStatus, ToStatus, ChangedBy, ChangedAt, Note | New: needed for disputes and the dashboard |
| Quotation | Id, JobRequestId, WorkerId, PriceType, Price, MinPrice, MaxPrice, DurationValue, DurationUnit, MaterialsIncluded, Notes, Status, Version, AmendsQuotationId, RowVersion | Range, materials, version and amendment fields added |
| AcceptedQuoteSnapshot | Id, JobRequestId, QuotationId, Version, SnapshotJson, AcceptedAt | New: immutable record (QTE-05) |

### 9.5 Messaging

| Entity | Key fields | Notes |
| --- | --- | --- |
| Conversation | Id, JobRequestId, CustomerId, WorkerId, IsReadOnly, LastMessageAt | Unique index on (JobRequestId, CustomerId, WorkerId) |
| Message | Id, ConversationId, SenderId, BodyOriginal, BodyMasked, SentAt, DeliveredAt, ReadAt | Original and masked text both stored (CHT-07) |
| MessageAttachment | Id, MessageId, Url, SizeBytes | New |

### 9.6 Payments, reviews, disputes, reports

| Entity | Key fields | Notes |
| --- | --- | --- |
| Payment | Id, JobRequestId, Method, WorkerAmount, CustomerAmount, Amount, PlatformFee, Status (AwaitingConfirmation, Confirmed, Mismatch), CreatedAt, ConfirmedAt | Two amounts for dual confirmation |
| Review | Id, JobRequestId, ReviewerId, RevieweeId, Stars, Comment, Reply, RepliedAt, IsHidden, HiddenReason, HiddenBy, EditableUntil | Unique on (JobRequestId, ReviewerId) |
| Dispute | Id, JobRequestId, OpenedBy, Reason, Description, Status, PreviousRequestStatus, AssignedAdminId, Decision, FinalRequestStatus, ResolvedAt |  |
| DisputeEvidence | Id, DisputeId, UploadedBy, StoragePath | New |
| DisputeOutcome | Id, DisputeId, Type (Warning, ReviewRemoval, Suspension), TargetUserId, SuspendedUntil | New |
| Report | Id, ReporterId, TargetType (Message, Review, User), TargetId, Reason, Note, Status, HandledBy, Action | New (RPT) |

### 9.7 Notifications and audit

| Entity | Key fields | Notes |
| --- | --- | --- |
| Notification | Id, UserId, Type, Title, Body, Link, IsRead, CreatedAt, EmailSentAt |  |
| NotificationPreference | UserId, Category, InApp, Email | New (NTF-04) |
| AuditLog | Id, ActorId, Action, TargetType, TargetId, DetailsJson, IpAddress, CreatedAt | Append only |

## 10. Risks

| Technical risk | Mitigation |
| --- | --- |
| Two quotes accepted for the same request at the same moment | Transaction + RowVersion (BR-04); an integration test fires two accepts in parallel and expects one 409 Conflict |
| A user reads or writes a chat they do not belong to through SignalR | Hub authenticated with the same JWT; ownership check in every hub method; tests per method |
| Background jobs run twice or are missed (expiry, auto-complete, digests) | Hangfire jobs are idempotent: each re-reads the current state and does nothing if the transition no longer applies |
| Leak of identity documents | Encryption at rest, private storage, admin-only endpoint, audit log, fake data only |
| Contact masking is bypassed | Unit tests with sample messages (Latin and Arabic-Indic digits, spaced numbers, links); report feature as backstop |
| OTP emails not received during the demo | Mailpit in development; deployed demo uses a transactional email provider with SPF/DKIM; resend option |
| Wrong dates around midnight or DST | Store UTC, convert to Africa/Cairo only in the UI (USE-05) |
| Scope too large for the team and timeline | Build Must items end to end first; Should and Could are stretch goals (Section 11) |

## 11. Release plan

| Release | Content |
| --- | --- |
| MVP (graduation demo) | All **Must** items: Auth, Verification, Profiles, Discovery, Open and Direct requests, Chat, Quotations, Cash payment confirmation, Reviews, basic Disputes and Reports, in-app + email Notifications, basic Admin |
| MVP+ (if time allows) | **Should** items: read receipts, amendments, notification preferences and digests, dashboard, CI, performance seed and tests |
| Release 2 | Online payment gateway, push notifications, advanced disputes, criminal-record check, weekly schedule |
| Release 3 | Commissions, subscriptions, urgent requests, mobile app, SMS |

Suggested build order: Identity → Locations & Professions → Worker profile + Verification → Requests → Quotations → Chat → Completion & Payment → Reviews → Notifications → Admin → Disputes & Reports.

## 12. Decisions

Every question left open in v0.2 is decided below. Because the goal is a graduation project, each choice favours the cleaner technical design; real-world launch topics (launch city, legal review, commercial availability targets) are out of scope.

| # | Decision | Why | See |
| --- | --- | --- | --- |
| D1 | Criminal-record check is built but off | Kept from v0.2 | VER-02 |
| D2 | Workers see all matching requests, no daily limit | Kept from v0.2 | REQ-10 |
| D3 | OTP and notifications by email; SMS only behind `ISmsProvider` | Kept from v0.2 | NTF-06 |
| D4 | No commission; `PlatformFee` stays 0 | Kept from v0.2 | PAY-04 |
| D5 | One role per account | Simpler auth and authorization rules; no role switching | 2.1 |
| D6 | Busy workers are hidden from search and get no new-request alerts, but can still view the feed and quote | Availability then means "don't push work to me", not "lock me out" | PRO-05 |
| D7 | On an open request only the worker starts a conversation; a direct request creates its conversation | Every conversation always has a request and a clear initiator | CHT-01 |
| D8 | Phone numbers and address are shown automatically after Agreed | The worker must reach the customer; no extra consent flow to build | CHT-09, BR-06 |
| D9 | Only the assigned worker moves a request to InProgress | One owner per transition keeps the state machine unambiguous | T5 |
| D10 | If either party cancels after Agreed, the request ends Cancelled; the customer re-posts it as a new request | Terminal states stay terminal; history stays clean | T6 |
| D11 | Cancelling during InProgress is allowed with a mandatory reason; money disagreements go through a dispute | The system cannot verify partial payments, so it does not try | T11 |
| D12 | Reviews are blind: published when both submit or the window ends | Prevents retaliation reviews | REV-06 |
| D13 | All values in Section 7 are final defaults | One source of truth for code and tests | 7 |
| D14 | Verification documents are deleted by a Hangfire job 90 days after the final decision | Minimises stored sensitive data | PRV-04 |
| D15 | Email: Mailpit locally, a transactional provider's free tier for the deployed demo, both behind `IEmailSender` | No cost, testable locally | 8 |

**Demo seed data**

- City: New Cairo, with four districts (First Settlement, Fifth Settlement, El Rehab, Madinaty).
- Professions: Plumbing, Electrical, Carpentry, AC repair, Painting.
- Accounts: 1 admin, 3 customers, 10 workers (8 Approved, 1 Pending, 1 Rejected), all with fake identity data.
- Requests: at least one request in every state of Section 3.1, so each screen and transition can be shown live.
