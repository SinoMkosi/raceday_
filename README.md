# RaceDay: Part 3 (MVC Web Application)

RaceDay lets Organisers create running, walking and cycling events, and lets Participants enrol and view their results.

## Projects
- **RaceDay.Api**: the Part 2 REST API (database, sessions, Azure Blob Storage endpoints)
- **RaceDay.Web**: the ASP.NET Core MVC front end. It calls the API only and has no database access.
- **RaceDayTests**: unit tests

## What works
- Register, log in and log out, with session-based roles (Organiser and Participant)
- Organiser: dashboard, create/edit/delete events, categories, enrolments (confirm), results entry
- Participant: events list with filters, event details, enrol, my enrolments, my results, profile
- Enrolment status colours: green = Confirmed, amber = Pending
- Azure Blob Storage: Organisers upload event banners and Participants upload profile pictures. The website sends the file to the API, the API stores it in Azure Blob Storage, and the website never talks to Azure directly.

## MVC structure
Controllers in RaceDay.Web call the API through one `ApiClient` class, which keeps the API session cookie. Models carry the data and Razor views display it.

## Run with Docker
Start RaceDay.Api in Visual Studio first (it listens on https://localhost:7228). Then, from the `Part2/RaceDay.web` folder:

```
docker build -t raceday-web .
docker run --rm -p 8080:8080 -e ApiBaseUrl=https://host.docker.internal:7228/ -e AllowInvalidApiCertificate=true raceday-web
```
Open http://localhost:8080. The API must be running on the host machine. The AllowInvalidApiCertificate setting is only for local Docker runs, because the API uses a development certificate. The containerised site behaves the same as the one run locally.

## Run locally
Open `Part2/RaceDay.Api/RaceDay.Api.slnx` in Visual Studio, set **RaceDay.Api** and **RaceDay.Web** as multiple startup projects, and start. Register an Organiser and a Participant account to try it.

## Demo video



---
# Part 2
# RaceDay System

A database and API planning project for an event registration and results
platform, built as part of [module code/assignment name].

## Overview

RaceDay lets Organisers create events and categories, Participants enrol
in categories, and Results get captured per enrolment. This repository
contains the planning and design deliverables for Part 1: the ERD, the
API endpoint plan, and the SQL database script.

## Repository Structure
## Entity Relationship Diagram

See docs/erd.png. The data model includes 6 entities: Roles, Users,
Events, Categories, Enrolments, and Results, with primary keys, foreign
keys, and cardinality shown on each relationship.

## API Endpoint Plan

See docs/api-endpoint-plan.md for the full table of endpoints covering
Authentication, User Profile, Events, Categories, Event Enrolments, and
Results.

## SQL Database Script

See docs/raceday_schema.sql. This script creates the full schema
matching the ERD exactly, with primary keys, foreign keys, and
constraints (NOT NULL, UNIQUE, DEFAULT), and seeds the database with:
- 2 Organisers and 2 Participants
- 3 Events
- Categories for each event
- Sample enrolments and a result

## CI/CD

A GitHub Actions workflow (.github/workflows/main.yml) runs on every
push and validates that the repository structure is correct — checking
that /docs exists and contains the ERD, API plan, and SQL script, and
that a root README.md is present.

*Latest successful build:*
<img width="1356" height="654" alt="Screenshot 2026-09-04 231155" src="https://github.com/user-attachments/assets/4989372c-31d3-4542-a152-ae3d1c4947e3" />
<img width="1359" height="717" alt="Screenshot 2026-09-04 212027" src="https://github.com/user-attachments/assets/13d6edd2-3f1e-411a-a0c8-c515e24cd10c" />


## Notes / Deviations

The SQL script in docs/raceday_schema.sql matches the ERD in docs/erd.png exactly — no
deviations.

## Author

Sinoxolo Mkosi | ST10445407
