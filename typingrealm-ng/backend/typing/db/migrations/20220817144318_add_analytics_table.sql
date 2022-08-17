-- migrate:up
create table "typing_bundle_analytics" (
    "id" bigserial primary key not null,
    "typing_bundle_id" bigserial not null references "typing_bundle"("id"),
    "total_characters_count" integer not null,
    "error_characters_count" integer not null,
    "total_time_ms" integer not null
);

create index "typing_bundle_analytics_typing_bundle_id_idx" on "typing_bundle_analytics" ("typing_bundle_id");

-- migrate:down
drop table "typing_bundle_analytics";
