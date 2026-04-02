# AGENTS.md

## Purpose

This file defines the working rules, coding standards, and delivery expectations for any AI agent contributing to this project.

The project is a **technical interview project**, not a fake enterprise platform. Every implementation must optimize for:

- clarity
- correctness
- interview defendability
- maintainability
- realistic scope

The main goal is to build a **clean, explainable, modular monolith** for an **Internal Ticket Manager** using **.NET 8 Web API + Angular**.

---

## Project intent

This codebase exists to demonstrate:

- strong backend fundamentals in C# / .NET
- good API design
- practical SQL and EF Core usage
- modern but reasonable Angular usage
- validation, testing, and documentation
- technical judgment without overengineering

Agents must never optimize for “looking advanced” over “being solid and defendable”.

---

## Non-negotiable principles

### 1. No overengineering
Do not introduce complexity that the project does not need.

Avoid unless clearly justified:

- microservices
- CQRS by default
- event bus
- MediatR just for fashion
- forced DDD
- rigid hexagonal architecture
- NgRx for trivial state
- excessive abstractions

### 2. Preserve behavior and intent
When modifying existing code:

- keep the original behavior unless a change is explicitly required
- do not invent functional changes
- do not silently change flows, validations, or data contracts
- keep changes aligned with existing patterns already chosen for the project

### 3. Prefer small, finished increments
A smaller but complete feature is better than a broad, half-finished feature.

### 4. Code must be explainable in interview
Every important decision should be easy to answer in plain language:

- why this approach was chosen
- what problem it solves
- what would be improved with more time

### 5. Backend quality has priority
The backend must feel especially solid. The frontend should be modern and clean, but should not become the place where unnecessary complexity appears.

---

## Delivery mindset for all agents

Before implementing anything, verify:

1. Is this necessary for the MVP?
2. Is this the simplest defendable solution?
3. Does this improve clarity?
4. Can this be explained in under two minutes?

When in doubt, prefer:

- simpler architecture
- fewer layers
- explicit code
- readable names
- focused tests

---

## Architecture standards

## Preferred architecture
Use a **modular monolith** with clear separation of responsibilities.

Recommended backend structure:

- `Api`
- `Application` or `Services`
- `Domain`
- `Infrastructure`

A slightly simpler structure is also acceptable if consistently applied:

- `Controllers`
- `Services`
- `Data`
- `Entities`
- `Dtos`
- `Validators`

### Architecture rules

- Use separation by responsibility, not abstraction for its own sake.
- Do not create extra layers unless they remove real duplication or improve clarity.
- Repositories are optional, not mandatory. If EF Core already provides enough abstraction through `DbContext`, do not add a repository layer just to look enterprise.
- Controllers must stay thin.
- Business logic must not be buried inside controllers.
- DTOs must be separate from persistence entities.
- Cross-cutting concerns should be centralized when reasonable, such as exception handling and auth.

---

## General coding standards

### Naming

- Use clear, boring, descriptive names.
- Prefer domain language over technical vanity names.
- Avoid abbreviations unless they are industry standard.
- Methods should describe intent precisely.
- Boolean names should read naturally, such as `isDeleted`, `isAuthenticated`, `hasAccess`.

### Readability

- Prefer straightforward code over clever code.
- Keep methods focused.
- Avoid deep nesting when guard clauses improve readability.
- Avoid long files with mixed responsibilities.
- Avoid comments that explain obvious syntax.
- Add comments only when they explain intent, a trade-off, or a non-obvious decision.

### Consistency

- Follow the conventions already present in the project.
- If there are two valid styles, pick one and stay consistent.
- Do not mix patterns randomly across files.

### Error handling

- Do not swallow exceptions silently.
- Do not expose internal exception details to API consumers.
- Use controlled error responses.
- Log meaningful failures.

### Increment safety

- Do not perform speculative refactors unless required.
- Do not rename broad parts of the codebase without a concrete benefit.
- Keep diffs focused.

---

## Backend standards (.NET 8 / ASP.NET Core)

## API design

- Build REST endpoints that are predictable and consistent.
- Use nouns for resources.
- Use proper HTTP verbs.
- Return coherent HTTP status codes.
- Keep request and response DTOs explicit.
- Never expose EF entities directly from controllers.
- Filtering and pagination should be supported cleanly in ticket listing endpoints.

### Controller rules

- Controllers orchestrate, they do not contain business logic.
- Validate incoming models properly.
- Delegate real work to services or application layer.
- Use route design that is simple and interview-friendly.

### Service rules

- Application services should contain business use cases.
- Each service method should do one clear thing.
- Avoid god services.
- Avoid mixing unrelated concerns in one service.

### DTO rules

- Separate create, update, detail, and list DTOs when that improves clarity.
- Keep DTOs minimal and purposeful.
- Do not over-reuse DTOs across unrelated operations.

---

## C# coding standards

### Async/await

- Use async all the way where I/O is involved.
- Do not block async calls with `.Result` or `.Wait()`.
- Name async methods with the `Async` suffix.
- Pass `CancellationToken` on public async application boundaries where it adds value.

### Dependency injection

- Use constructor injection.
- Choose service lifetimes deliberately:
  - `Scoped` for request/database-related services
  - `Singleton` only for stateless and thread-safe services
  - `Transient` for lightweight stateless helpers when appropriate
- Do not resolve services manually unless there is a strong reason.

### Nullability and validation

- Respect nullable reference types.
- Validate external input explicitly.
- Prefer failing fast on invalid state.

### Enums

- Use enums for stable domain concepts like ticket status, priority, and role.
- Do not use magic integers or strings for core domain state.

### Mapping

- Keep mapping explicit and readable.
- AutoMapper is optional, not required.
- If manual mapping is simpler and clearer, use manual mapping.

---

## EF Core and data access standards

### EF Core usage

- Use EF Core in a straightforward, idiomatic way.
- Keep queries readable.
- Understand when execution happens.
- Avoid premature materialization.
- Use `AsNoTracking()` for read-only queries where entities will not be updated.

### Query design

- Avoid N+1 query patterns.
- Use pagination for list endpoints.
- Filter at database level whenever possible.
- Only include what the endpoint actually needs.

### Migrations and schema

- Keep migrations clean and meaningful.
- Use sensible constraints and lengths.
- Model relationships explicitly.

### Database rules

- Prefer simple relational design.
- Avoid unnecessary denormalization.
- Be intentional with indexes if added.
- Use transactions only when they solve a real multi-step consistency need.

---

## Validation standards

- All external input must be validated.
- Validation messages should be clear and user-oriented.
- Validation should happen before persistence.
- FluentValidation is welcome if it keeps validation clean, but do not introduce it if the project is already simpler without it.

Typical validations expected:

- required fields
- string lengths
- enum validity
- referential integrity checks when needed
- basic business rules such as valid status transitions if implemented

---

## Authentication and authorization standards

- Use JWT for authentication.
- Keep auth implementation simple and defendable.
- Protect endpoints appropriately.
- Role usage should remain minimal and clear.
- Do not build an oversized security system for a demo project.

Minimum role model:

- `Admin`
- `Developer`

---

## Error handling and logging standards

- Use centralized exception handling where possible.
- Return controlled API error payloads.
- Avoid leaking stack traces or internal implementation details.
- Log errors with enough context to debug real issues.
- Do not over-log noise.

Good logging targets:

- authentication failures
- unexpected service exceptions
- invalid business actions when relevant
- important operational events only if they add value

---

## Frontend standards (Angular)

## Angular approach
Use **modern Angular**, but in a restrained and defendable way.

Preferred direction:

- standalone components
- Angular Router
- Reactive Forms
- HTTP services
- guards
- interceptors
- RxJS

Use signals only if they genuinely improve local clarity and do not make the project harder to explain.

Do not introduce NgRx unless the project genuinely reaches a state complexity that justifies it.

### Component standards

- Components should have one clear responsibility.
- Keep presentation and data orchestration reasonably separated.
- Avoid giant smart components.
- Prefer reusable UI patterns only when reuse is real.

### State management

- Default to local component state + services.
- Use RxJS streams for HTTP and async flows.
- Keep shared state simple.
- Do not build a global store for trivial filters or form state.

### RxJS standards

- Avoid nested subscriptions.
- Prefer operators such as `map`, `switchMap`, `catchError`, `tap` when appropriate.
- Handle error states intentionally.
- Clean up subscriptions properly when needed.
- Prefer patterns that are easy to explain.

### Forms

- Use Reactive Forms.
- Put validations in the form model.
- Display validation feedback clearly.
- Keep form models aligned with backend DTOs where reasonable.

### Routing and guards

- Routes should be predictable and simple.
- Protect private routes with an auth guard.
- Keep guard logic focused.

### Interceptors

- Use interceptors for cross-cutting HTTP concerns like attaching JWT tokens and handling common auth errors.
- Do not push feature-specific business logic into interceptors.

### UI standards

- Prioritize clarity and usability over visual complexity.
- A clean Bootstrap or Angular Material UI is enough.
- Accessibility basics matter:
  - proper labels
  - buttons that read clearly
  - semantic structure
  - visible validation states

---

## TypeScript standards

- Use explicit types where they improve clarity.
- Avoid `any` unless truly unavoidable.
- Prefer interfaces or types for DTO contracts.
- Keep frontend models aligned with backend contracts.
- Use clear file names and class names.

---

## Testing standards

## General testing policy
This project does **not** need huge coverage. It **does** need a small set of meaningful tests that prove understanding.

### Backend tests
Prioritize tests around:

- ticket creation validation
- status changes
- ticket filtering/pagination
- service logic with real business value

### Frontend tests
Prioritize tests around:

- form validation
- HTTP service behavior
- component behavior with meaningful state changes

### Testing rules

- Test behavior, not framework internals.
- Prefer a few relevant tests over many shallow ones.
- Keep tests readable.
- Test names should describe expected behavior clearly.
- Do not write fragile tests that depend on incidental implementation details.

---

## Documentation standards

Every agent must leave the project easier to understand.

### README expectations
The final project README should include:

- project overview
- stack
- architecture summary
- setup instructions
- environment/config notes
- main endpoints
- main frontend screens
- testing notes
- technical decisions
- future improvements

### Documentation tone

- professional
- realistic
- specific
- no fake enterprise language

Do not describe a demo as a massive production platform.

---

## Code review checklist for agents

Before considering a task done, verify:

- Does the implementation match the MVP and nothing more?
- Is the solution simpler than the previous idea, not more complex?
- Are names clear?
- Are controllers thin?
- Are DTOs separated from entities?
- Are validations present?
- Are error responses controlled?
- Is async used correctly?
- Are EF Core queries efficient enough for this scope?
- Is Angular state management still simple?
- Is the code easy to explain in interview?

---

## Agent-specific responsibilities

## Agent 1 — Architecture and planning

- Define a structure that is clean, minimal, and scalable enough for the MVP.
- Do not invent complexity.
- Produce a folder tree and technical backlog that support incremental delivery.

## Agent 2 — Backend API

- Build a solid .NET 8 Web API.
- Prioritize correctness, validation, auth, REST design, pagination, and Swagger.
- Keep services and controllers clean.
- Add only a few strong tests.

## Agent 3 — Frontend Angular

- Build a clean Angular application connected to the backend.
- Use routing, guard, interceptor, services, and Reactive Forms properly.
- Keep the UI simple, readable, and presentable.
- Avoid architecture inflation.

## Agent 4 — QA and validation

- Check main flows and edge cases.
- Review form and API validation paths.
- Produce a Postman collection and a bug/improvement list.

## Agent 5 — Documentation and interview preparation

- Write README and technical explanation materials.
- Help frame the project as an interview demo, not as an overblown platform.
- Summarize trade-offs and future improvements honestly.

---

## Definition of done

A task is done only when:

- the feature works
- the implementation is coherent with this file
- the code is readable
- the solution is defendable in interview
- no unnecessary complexity was added
- relevant validations exist
- errors are handled sensibly
- documentation is updated when needed

---

## Final rule

Build the project in a way that lets the owner say this truthfully in interview:

> I built this to demonstrate strong fundamentals, clean API design, practical Angular usage, validation, persistence, and testing. I intentionally avoided fake complexity and focused on delivering a clean, explainable solution.

If a change makes that statement weaker, do not make that change.
