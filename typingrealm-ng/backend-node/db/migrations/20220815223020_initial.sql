-- migrate:up
create table "typing_bundle" (
    "id" bigserial primary key not null,
    "created_at" timestamptz not null,
    "created_perf" integer not null,
    "text" varchar(5000) not null,
    "profile_id" varchar(500) not null
);

create table "event" (
    "id" bigserial primary key not null,
    "typing_bundle_id" bigserial not null references "typing_bundle"("id"),
    "order_index" integer not null,
    "index" integer not null,
    "eventType" varchar(50) not null,
    "key" varchar(50) not null,
    "perf" integer not null
);

create index "typing_bundle_id_idx" on "event" ("typing_bundle_id");

-- migrate:down
drop table "event", "typing_bundle";
