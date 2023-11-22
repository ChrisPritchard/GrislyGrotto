
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

use comrak::plugins;
use prelude::*;

pub use mapping::prev_next_month;
use time::{macros::format_description, format_description::FormatItem, PrimitiveDateTime};

use crate::local_time;

const DEFAULT_DATABASE_PATH: &str = "./grislygrotto.db";

fn markdown_to_html(markdown: &str) -> String {
    let mut markdown_options = comrak::ComrakOptions::default();
    markdown_options.render.unsafe_ = true; // required for html rendering, e.g. images

    let mut markdown_plugins = comrak::ComrakPlugins::default();
    let syntect = plugins::syntect::SyntectAdapter::new("base16-ocean.dark");
    markdown_plugins.render.codefence_syntax_highlighter = Some(&syntect);

    comrak::markdown_to_html_with_plugins(markdown, &markdown_options, &markdown_plugins)
}

fn db() -> Result<sqlite::Connection> {
    let mut args = env::args();
    let path = args.nth(1).unwrap_or(DEFAULT_DATABASE_PATH.into());
    Ok(sqlite::open(path)?)
}

// for the below, https://time-rs.github.io/book/api/format-description.html

fn storage_date_format_1() -> &'static [FormatItem<'static>] {
    format_description!("[year]-[month]-[day] [hour]:[minute]:[second]")
}

fn storage_date_format_2() -> &'static [FormatItem<'static>] {
    format_description!("[year]-[month]-[day] [hour]:[minute]:[second].[subsecond]")
}

fn storage_display_format() -> &'static [FormatItem<'static>] {
    format_description!("[hour repr:12]:[minute] [period], on [weekday], [day] [month repr:long] [year]")
}

fn current_datetime_for_storage() -> String {
    let format = storage_date_format_1();
    let local_time = local_time::get_now();
    local_time.format(format).unwrap()
}

fn storage_datetime_as_display(datetime: &str) -> Result<String> {
    let format = if datetime.contains(".") { 
        storage_date_format_2() 
    } else { 
        storage_date_format_1()
    };
    let parsed = PrimitiveDateTime::parse(datetime, format);
    if let Err(err) = parsed {
        log::error!("error parsing date value '{}', error: {}", datetime, err);
        return Err(err.into())
    }
    let result = parsed.unwrap().format(storage_display_format()).unwrap();
    Ok(result)
}