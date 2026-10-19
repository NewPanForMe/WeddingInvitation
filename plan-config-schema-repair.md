# Plan: Config Schema Repair

**Created**: 2026-10-19 13:10:00
**Scope**: Local PostgreSQL schema, API startup initialization, API configuration endpoint verification
**Risk Level**: medium
**Rollback Strategy**: The database changes only add nullable-safe/defaulted columns. If the release must be reverted, deploy the previous API binary; the added columns are ignored by the prior version.

## Summary

The running API now maps the `cover_show_in_moments` and `photo_type` fields introduced for photo classification, while the existing PostgreSQL schema does not yet contain them. This causes PostgreSQL error `42703` and a 500 response from `GET /api/admin/config`.

The repair will apply the idempotent schema additions already recorded in `db/001_init.sql`, then add an API startup guard so a future compatible upgrade cannot expose the endpoint before those columns exist.

## Steps

### Step 1: Add startup schema guard
- **Operation**: edit
- **Target**: `server/src/WeddingInvitation.Infrastructure/Persistence/DbInitializer.cs`
- **Description**: Before entity queries, execute the two `ALTER TABLE ... ADD COLUMN IF NOT EXISTS` statements and migrate an existing configured cover URL to the `cover` photo type.
- **Expected Result**: Future API starts make this backward-compatible schema update before handling configuration requests.

### Step 2: Apply the schema repair to the active local database
- **Operation**: execute
- **Target**: Local PostgreSQL database configured by `appsettings.Development.json`
- **Description**: Use the same idempotent SQL to create `invitation_config.cover_show_in_moments` and `photos.photo_type`, then mark the configured cover photo when present.
- **Expected Result**: The active database matches the current entity model without deleting or overwriting photos or configuration values.

### Step 3: Build and verify the endpoint
- **Operation**: execute
- **Target**: `server/WeddingInvitation.sln` and `http://localhost:5080/api/admin/config`
- **Description**: Build the API, restart its local process if required, and verify the endpoint no longer fails because of missing columns.
- **Expected Result**: Release build succeeds and the API progresses past the prior PostgreSQL `42703` error.
