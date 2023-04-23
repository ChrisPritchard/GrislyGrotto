
mod sql;
mod mapping;
mod view_posts;
mod comments;
mod archives;

pub use view_posts::get_latest_posts;
pub use view_posts::get_single_post;
pub use comments::comment_count;
pub use comments::add_comment;

mod prelude {
    pub use sqlite::{Value, State, Statement};
    pub use anyhow::Result;
}

use prelude::*;

const DATABASE_PATH: &str = "./grislygrotto.db";
const STORAGE_DATE_FORMAT: &str = "%Y-%m-%d %H:%M:%S";
const STORAGE_DISPLAY_FORMAT: &str = "%l:%M %p, on %A, %e %B %Y";

fn markdown_options() -> comrak::ComrakOptions {
    let mut markdown_options = comrak::ComrakOptions::default();
    markdown_options.render.unsafe_ = true;

    markdown_options
}

fn db() -> Result<sqlite::Connection> {
    Ok(sqlite::open(DATABASE_PATH)?)
}

fn current_datetime_for_storage() -> String {
    format!("{}", chrono::offset::Local::now().format(STORAGE_DATE_FORMAT))
}

fn storage_datetime_as_display(datetime: &str) -> Result<String> {
    let parsed = chrono::NaiveDateTime::parse_from_str(datetime, STORAGE_DATE_FORMAT)?;
    Ok(format!("{}", parsed.format(STORAGE_DISPLAY_FORMAT)))
}