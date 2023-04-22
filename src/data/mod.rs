
mod sql;
mod mapping;
mod view_posts;
mod comments;

pub use view_posts::get_latest_posts;
pub use view_posts::get_single_post;
pub use comments::add_comment;

mod prelude {
    pub use sqlite::{Value, State, Error};
}

const DATABASE_PATH: &str = "./grislygrotto.db";

fn markdown_options() -> comrak::ComrakOptions {
    let mut markdown_options = comrak::ComrakOptions::default();
    markdown_options.render.unsafe_ = true;

    markdown_options
}

fn db() -> Result<sqlite::Connection, sqlite::Error> {
    Ok(sqlite::open(DATABASE_PATH)?)
}
