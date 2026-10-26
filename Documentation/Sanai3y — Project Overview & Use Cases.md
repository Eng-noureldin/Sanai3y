# Sanai3y — Project Overview & Use Cases

Oct 9, 2026&#32;

Sanai3y is a web platform where a customer posts a home-repair problem, verified tradesmen in the same area send price quotes, and the customer picks one, marks the job done and rates the worker.

## 1. Overview

**Problem.** People who move to new cities have no word-of-mouth network, so finding a trustworthy plumber or electrician at a fair price is hard.

**Solution.** The customer describes the problem once; verified workers in that district send quotes; the customer compares them, accepts one, and rates the worker when the job is done.

**Project goal.** A DEPI Full Stack .NET graduation project.

**In scope**

- Register and log in as Customer or Worker (Admin is seeded)
- Worker verification: upload ID photo, admin approves or rejects
- Worker profile: bio, experience, professions, city and districts, availability
- Search workers by city, district and profession
- Customer posts a request; matching workers see it
- Workers send one fixed-price quote each; customer accepts exactly one
- Simple text chat between the customer and a worker about a request
- Customer marks the job completed and rates the worker (1–5 stars + comment)
- In-app notifications (bell icon)
- Admin: verify workers, manage cities, districts and professions, block users, hide reviews

**Not in this version:** see Section 7.

## 2. Actors and features

| Actor | Can do |
| --- | --- |
| Guest | Browse cities, professions and verified worker profiles; register |
| Customer | Post requests, see quotes, chat with workers, accept one quote, mark the job completed, rate the worker, cancel an open request |
| Worker | Edit profile, upload ID for verification, set availability, see matching requests, send / edit / withdraw a quote, chat with customers |
| Admin | Approve or reject workers, manage cities, districts and professions, block users, hide reviews, see simple counts |
| System | A background service that marks old open requests as Expired |

**Modules (one folder each):** Auth · Lookups · Workers · Requests · Quotes · Chat · Reviews · Notifications · Admin

## 3. Use case diagram

&#91;embedded content: use case diagram · 4 actors, 9 use cases\]

The customer and the worker meet only in the chat; everything else is one actor's job, which keeps each screen simple.

## 4. Use cases

### UC-01 Register and log in

**Actor:** Guest · **Result:** logged-in Customer or Worker

1. Guest enters full name, email, mobile number, password and chooses Customer or Worker.
2. System checks the email is not used, creates the account and returns a JWT.
3. Later the user logs in with email and password and gets a new JWT.

**Alternatives:** email already used → error · wrong password → "Invalid email or password" · blocked user → login refused.

### UC-02 Get verified as a worker

**Actor:** Worker, Admin · **Result:** worker Approved and visible in search

1. Worker uploads a photo of the national ID and a selfie with the ID.
2. System saves the files privately and sets the worker to Pending.
3. Admin opens the list of pending workers, views the photos and approves.
4. System sets the worker to Approved and sends a notification.

**Alternatives:** admin rejects with a reason → worker sees the reason and uploads again.

### UC-03 Find a worker

**Actor:** Guest or Customer · **Result:** list of workers

1. User picks a city, and optionally a district and a profession.
2. System shows Approved, available workers who serve that area, sorted by rating, 10 per page.
3. User opens a worker's profile to see bio, professions and reviews.

### UC-04 Post a request

**Actor:** Customer · **Result:** request Open

1. Customer picks profession, city and district, writes a description and adds up to 3 photos.
2. System saves the request as Open, expiring after 7 days.
3. System notifies Approved workers with that profession who serve that district.

**Alternatives:** customer cancels the request while it is still Open → Cancelled.

### UC-05 Send a quote

**Actor:** Worker · **Precondition:** worker Approved, request Open and matching · **Result:** quote Pending

1. Worker opens the request from "Requests near me".
2. Worker enters a price (EGP), expected duration in days and a note.
3. System saves the quote and notifies the customer.

**Alternatives:** worker already quoted → edits the same quote · worker withdraws the quote before it is accepted.

### UC-06 Chat about a request

**Actor:** Customer, Worker · **Result:** messages exchanged

1. A worker (or the customer, from a quote) opens a chat on the request.
2. Either side sends text messages; they appear live for the other side.
3. The chat history stays on the request page.

### UC-07 Accept a quote

**Actor:** Customer · **Precondition:** request Open with at least one quote · **Result:** request Agreed

1. Customer sees all quotes in a table (price, days, worker rating).
2. Customer accepts one.
3. System, in one transaction, marks it Accepted, marks the others Rejected and sets the request to Agreed.
4. System notifies all workers who quoted; the accepted worker now sees the customer's phone number.

**Alternatives:** two accepts at the same moment → the second fails with "This request was already updated".

### UC-08 Complete and rate

**Actor:** Customer · **Precondition:** request Agreed · **Result:** request Completed and worker rated

1. After the job, the customer taps "Job completed".
2. System sets the request to Completed and adds 1 to the worker's completed jobs.
3. Customer gives 1–5 stars and an optional comment.
4. System saves the review and recalculates the worker's average rating.

**Alternatives:** either side cancels an Agreed request with a reason → Cancelled.

### UC-09 Manage the platform

**Actor:** Admin · **Result:** clean data and safe users

1. Admin adds or deactivates cities, districts and professions.
2. Admin blocks or unblocks a user.
3. Admin hides an abusive review; the rating is recalculated.

## 5. Request lifecycle and key rules

A request has five states: `Open → Agreed → Completed`, plus `Cancelled` and `Expired`.

| From | To | Who | When |
| --- | --- | --- | --- |
| — | Open | Customer | Posts the request |
| Open | Agreed | Customer | Accepts one quote |
| Open | Cancelled | Customer | Cancels before accepting |
| Open | Expired | System | 7 days pass with no accepted quote |
| Agreed | Completed | Customer | Taps "Job completed" |
| Agreed | Cancelled | Customer or worker | Cancels with a reason |

**Rules**

- Only Approved, unblocked workers appear in search, see requests and send quotes.
- One quote per worker per request; a quote can be edited or withdrawn only while it is Pending.
- Exactly one quote can be accepted per request (transaction + RowVersion).
- The customer's phone number is shown only to the accepted worker.
- Only the customer of a Completed request can review it, once.
- Completed-jobs count and average rating are calculated by the system, never typed in.

## 6. Tech stack

| Area | Choice |
| --- | --- |
| Backend | ASP.NET Core Web API (.NET 10), 3 layers: API → Services → Data |
| Auth | ASP.NET Core Identity + JWT (one token, valid 1 day) |
| Database | SQL Server + EF Core Code First |
| Chat and notifications | SignalR (one hub) |
| Background work | One `BackgroundService` that expires old requests every hour |
| Files | Saved on disk under `wwwroot/uploads`; ID photos in a private folder served only to admins |
| Frontend | React + TypeScript, React Router, Axios, Arabic RTL |
| API docs | Swagger |

Related team files: ERD, Class Design, Project Plan.

## 7. Future work

| Feature | Why it waits |
| --- | --- |
| Direct requests to one worker | Open requests already cover the main flow |
| Email OTP, refresh tokens, account lockout | Security hardening; not needed to demonstrate the flow |
| Price ranges, quote revisions and amendments | Fixed price is enough to compare offers |
| Job in progress, worker confirmation, auto-completion | One "Job completed" step is simpler and clear |
| Payment records and online payment | Cash outside the platform for now |
| Two-way and blind reviews, replies | Rating the worker is the valuable part |
| Disputes and reports | Admin can already block users and hide reviews |
| Email notifications, digests, preferences | In-app bell is enough for the demo |
| Read receipts, image messages, hiding phone numbers in chat | Plain text chat is enough |
| Clean Architecture with CQRS and domain events | 3 layers are easier for the team to learn and finish |
