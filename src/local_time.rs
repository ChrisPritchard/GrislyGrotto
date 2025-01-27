use time::{macros::offset, Month, OffsetDateTime};

// just used because time crate fails in NZ daylight savings time. should be replaced in the future maybe

pub fn get_now() -> OffsetDateTime {
    let utc_time = OffsetDateTime::now_utc();
    let month = utc_time.month() as u8;
    let offset = if month < Month::April as u8 || month > Month::September as u8 { offset!(+13) } else { offset!(+12) };
    let local_time = utc_time.to_offset(offset); // hack hack hack
    local_time
}