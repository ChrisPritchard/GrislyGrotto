
mod sql;
mod mapping;
pub mod view_posts;
pub mod comments;
pub mod archives;
pub mod search;
pub mod account;
pub mod editor;

mod prelude {
    pub use sqlite::{Value, State, Statement};
    pub use anyhow::Result;
}

use std::env;

use prelude::*;

pub use mapping::prev_next_month;

const DEFAULT_DATABASE_PATH: &str = "./grislygrotto.db";
const STORAGE_DATE_FORMAT_1: &str = "%Y-%m-%d %H:%M:%S";
const STORAGE_DATE_FORMAT_2: &str = "%Y-%m-%d %H:%M:%S.%f";
const STORAGE_DISPLAY_FORMAT: &str = "%l:%M %p, on %A, %e %B %Y";

fn markdown_options() -> comrak::ComrakOptions {
    let mut markdown_options = comrak::ComrakOptions::default();
    markdown_options.render.unsafe_ = true;

    return markdown_options
}

fn db() -> Result<sqlite::Connection> {
    let mut args = env::args();
    let path = args.nth(1).unwrap_or(DEFAULT_DATABASE_PATH.into());
    Ok(sqlite::open(path)?)
}

fn current_datetime_for_storage() -> String {
    format!("{}", chrono::offset::Local::now().format(STORAGE_DATE_FORMAT_1))
}

fn storage_datetime_as_display(datetime: &str) -> Result<String> {
    let format_string = if datetime.contains(".") { STORAGE_DATE_FORMAT_2 } else { STORAGE_DATE_FORMAT_1 };
    let parsed = chrono::NaiveDateTime::parse_from_str(datetime, format_string);
    if let Err(err) = parsed {
        log::error!("error parsing date value '{}', error: {}", datetime, err);
        return Err(err.into())
    }
    Ok(format!("{}", parsed.unwrap().format(STORAGE_DISPLAY_FORMAT)))
}