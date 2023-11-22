use time::OffsetDateTime;

// just used because time crate fails in NZ daylight savings time. should be replaced in the future maybe

pub fn get_now() -> OffsetDateTime {
    let utc_time = OffsetDateTime::now_utc();
    let local_time = utc_time.to_offset(time::macros::offset!(+13)); // hack hack hack
    local_time
}