# RaceDay System — API Endpoint Plan

This plan covers Authentication, User Profile, Events, Categories, Event Enrolments, and Results,
matching the entities in `erd.png`. Roles: **Admin**, **Organiser**, **Participant**.

## Authentication

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/auth/register` | Register a new user (Participant or Organiser) | Public | `{ fullName, email, password, phoneNumber, role }` | `201 Created` – `{ userId, email, role, token }` |
| POST | `/api/auth/login` | Authenticate a user and issue a token | Public | `{ email, password }` | `200 OK` – `{ userId, role, token, expiresAt }` |
| POST | `/api/auth/logout` | Invalidate the current session/token | Authenticated | *(none)* | `204 No Content` |

## User Profile

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/users/me` | Get the logged-in user's profile | Authenticated | *(none)* | `200 OK` – user object |
| PUT | `/api/users/me` | Update the logged-in user's profile | Authenticated | `{ fullName, phoneNumber }` | `200 OK` – updated user object |
| PUT | `/api/users/me/password` | Change password | Authenticated | `{ currentPassword, newPassword }` | `200 OK` – `{ message }` |
| GET | `/api/users` | List all users | Admin | *(none)* | `200 OK` – array of user objects |
| GET | `/api/users/{id}` | Get a specific user's profile | Admin | *(none)* | `200 OK` – user object |
| DELETE | `/api/users/{id}` | Deactivate/remove a user | Admin | *(none)* | `204 No Content` |

## Events

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/events` | List all events (supports filtering by status/date) | Public | *(none)* | `200 OK` – array of event objects |
| GET | `/api/events/{id}` | Get details of a single event, including its categories | Public | *(none)* | `200 OK` – event object with nested categories |
| POST | `/api/events` | Create a new event | Organiser | `{ eventName, description, eventDate, location }` | `201 Created` – event object |
| PUT | `/api/events/{id}` | Update an event (only the owning organiser) | Organiser (owner) / Admin | `{ eventName, description, eventDate, location, status }` | `200 OK` – updated event object |
| DELETE | `/api/events/{id}` | Cancel/delete an event | Organiser (owner) / Admin | *(none)* | `204 No Content` |
| GET | `/api/events/mine` | List events created by the logged-in organiser | Organiser | *(none)* | `200 OK` – array of event objects |

## Categories

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/events/{eventId}/categories` | List categories for an event | Public | *(none)* | `200 OK` – array of category objects |
| GET | `/api/categories/{id}` | Get a single category's details | Public | *(none)* | `200 OK` – category object |
| POST | `/api/events/{eventId}/categories` | Add a category to an event | Organiser (owner) | `{ categoryName, distanceKm, maxParticipants, entryFee }` | `201 Created` – category object |
| PUT | `/api/categories/{id}` | Update a category | Organiser (owner) / Admin | `{ categoryName, distanceKm, maxParticipants, entryFee }` | `200 OK` – updated category object |
| DELETE | `/api/categories/{id}` | Remove a category | Organiser (owner) / Admin | *(none)* | `204 No Content` |

## Event Enrolments

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/categories/{categoryId}/enrolments` | Enrol the logged-in participant into a category | Participant | *(none, or `{ notes }`)* | `201 Created` – enrolment object with generated `bibNumber` |
| GET | `/api/enrolments/mine` | List the logged-in participant's enrolments | Participant | *(none)* | `200 OK` – array of enrolment objects |
| GET | `/api/categories/{categoryId}/enrolments` | List all enrolments for a category | Organiser (owner) / Admin | *(none)* | `200 OK` – array of enrolment objects |
| PUT | `/api/enrolments/{id}` | Update enrolment status (confirm/cancel) | Organiser (owner) / Admin | `{ status }` | `200 OK` – updated enrolment object |
| DELETE | `/api/enrolments/{id}` | Withdraw from an event | Participant (owner) / Admin | *(none)* | `204 No Content` |

## Results

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/categories/{categoryId}/results` | Get the results/leaderboard for a category | Public | *(none)* | `200 OK` – array of result objects, ordered by `position` |
| GET | `/api/enrolments/{enrolmentId}/result` | Get the result for a specific enrolment | Participant (owner) / Organiser / Admin | *(none)* | `200 OK` – result object |
| POST | `/api/enrolments/{enrolmentId}/result` | Capture a result for an enrolment | Organiser (owner) / Admin | `{ finishTime, position, status }` | `201 Created` – result object |
| PUT | `/api/results/{id}` | Update/correct a captured result | Organiser (owner) / Admin | `{ finishTime, position, status }` | `200 OK` – updated result object |
| DELETE | `/api/results/{id}` | Remove a result (e.g. entry error) | Admin | *(none)* | `204 No Content` |

---
**Notes**
- "Organiser (owner)" means the organiser who created the parent event; enforced server-side by comparing `OrganiserID` to the logged-in user.
- Error responses (`400`, `401`, `403`, `404`) are omitted above for brevity but apply to every endpoint per standard REST conventions.
