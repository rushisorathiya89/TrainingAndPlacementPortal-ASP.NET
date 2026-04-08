# Training & Placement Portal: Dynamic Workflow Implementation Guide

This document explains the logic and architecture behind the fully dynamic workflows we implemented: **JD Submission**, **Admin Approval**, and **Student Applications**. 

---

## 1. The Database Architecture (Models)
We started by setting up the foundation in the database. Three primary models connect this flow:

### `Company.cs`
- Holds the company profile and recruiter contact details.
- Includes a **`Status`** field (Pending, Verified, Rejected).
- Navigation logic: `public ICollection<JobPosting> JobPostings` connects a single company to multiple job postings.

### `JobPosting.cs`
- Holds the job-specific details (Role, CTC, Eligibility, Drive Dates).
- Also has a **`Status`** field (Pending, Approved, Rejected, OnHold). *Why both?* So that one company can post multiple jobs over time, and the Admin can approve/reject each job individually.
- Acts as the child of `Company` and parent of `JobApplication`.

### `JobApplication.cs`
- The pivot table that links a `Student` and a `JobPosting`.
- Holds the **`ApplicationStatus`** (Applied, Shortlisted, Selected, Rejected).

---

## 2. JD Submission Flow (Guest -> Pending)

**Logic:** A guest (recruiter) submits a form, and it is safely inserted into the database awaiting Admin review.

1. **Frontend (`SubmitJd.cshtml`)**: 
   - We removed the standard form `<form action="...">` postback.
   - We gave every form field an `id`.
   - The `handleJdSubmit()` JavaScript function collects these values into a JSON object.
   - We use the JavaScript `fetch()` API to send a `POST` request to the backend.
2. **Backend (`CompanyApiController.cs -> SubmitJd`)**:
   - The API receives the data via a `SubmitJdDto` object (Data Transfer Object).
   - It first checks if the Company already exists (by email). If not, it creates a new `Company`.
   - It then creates a new `JobPosting` linked to that company.
   - Both the Company and JobPosting are assigned the status **"Pending"** by default.

---

## 3. Admin Approval Flow (Pending -> Approved)

**Logic:** Authorized Admins can view all JDs, inspect details, and update the status of the job posting dynamically.

1. **Frontend (`CompanyManagement.cshtml`)**:
   - On page load, `loadCompanyCards()` calls `GET /api/company/job-postings`.
   - Instead of static HTML, the JavaScript iterates through the returned list and uses `document.createElement()` to build the HTML cards for every single job.
   - When the Admin clicks **Accept**, it triggers `updateJpStatus(id, 'Approved')`.
2. **Backend Status Toggling (`CompanyApiController.cs -> UpdateJobPostingStatus`)**:
   - Only accessible by `[Authorize(Roles = "Admin")]`.
   - Updates the `JobPosting.Status` to "Approved" and automatically keeps the parent `Company.Status` in sync ("Verified").
   - It instantly returns success to the UI. The UI removes the visual "Accept" button and turns the card badge green without needing a page refresh.
3. **Admin JD Edit (`JdDetail.cshtml`)**:
   - Reads the Job ID from the URL (`?id=X`).
   - Fetches the exact details and populates the textboxes.
   - Allows the Admin to override text (like fixing a typo in the package) and sends a `PUT /api/company/job-postings/{id}` request to overwrite the row in the DB.

---

## 4. Student Visibility (The Filtering Logic)

**Logic:** Students must only see jobs that the Admin has stamped as "Approved".

1. **Backend Filtering (`CompanyApiController.cs -> GetApprovedJobs`)**:
   - `[Authorize(Roles = "Student")]` protects the route.
   - The Entity Framework query specifically filters by status: 
     `Where(j => j.Status == "Approved" && j.IsActive)`
   - This ensures pending or held jobs never leak to the student panel.
2. **Frontend (`Student/Jobs.cshtml`)**:
   - Similar to the Admin panel, this fetches `GET /api/company/approved-jobs` on page load.
   - Renders out the cards for students to see.

---

## 5. Student Application Flow 

**Logic:** Authorized students click "Apply", which creates an immutable record linking them to the job.

1. **Backend (`ApplicationApiController.cs`)**:
   - **`POST apply/{jobId}`**: We figure out *who* the student is securely by extracting their `UserId` directly from their JWT Authentication Token (`User.FindFirst(ClaimTypes.NameIdentifier)`). We NEVER trust the frontend to tell us the user's ID because it could be manipulated.
   - We verify the job is still "Approved".
   - We verify the student hasn't already applied.
   - We insert a new `JobApplication` row set to "Applied" and timestamp it.
2. **Frontend Tracking (`Student/Jobs.cshtml` & `JobDetails.cshtml`)**:
   - When rendering the job cards, the UI silently calls `GET /api/applications/check/{jobId}`. 
   - If the backend returns `hasApplied: true`, the UI replaces the blue "Apply" button with a green, disabled "Applied ✅" button so they can't spam it.
3. **Application History (`Student/Applications.cshtml`)**:
   - Hits `GET /api/applications/my-applications`.
   - The backend uses `.Include(a => a.JobPosting).ThenInclude(jp => jp.Company)` to do a SQL JOIN. It gathers the Student's application, pairs it with the Job details, and pairs *that* with the Company details.
   - Returns a single flattened list so the UI can draw the history table easily.

---

### Key Takeaways of the Dynamic Architecture
1. **Single Page Application (SPA) Feel**: We used `fetch()` API for everything. This means clicks submit data and update the screen instantly without full-page white flashes.
2. **Separation of Concerns (MVC + API)**: The Razor Views (`.cshtml`) only handle the HTML layout and loading spinners. The logic (CRUD) is strictly maintained inside distinct API Controllers (`CompanyApiController`, `ApplicationApiController`).
3. **Role-based DB Logic**: Because EF Core handles all Database schema relationships natively, we rely heavily on its nested navigation tracking. To delete a company drops its jobs automatically, and dropping a job drops student applications—keeping the database perfectly clean.
