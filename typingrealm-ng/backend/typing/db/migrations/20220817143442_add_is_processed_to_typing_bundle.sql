-- migrate:up
alter table "typing_bundle"
add column "is_processed" boolean not null default false;

-- migrate:down
alter table "typing_bundle"
drop column "is_processed";
