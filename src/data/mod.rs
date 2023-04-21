use sqlite::{Value, State};

use crate::model::*;

mod sql;
mod mapping;

pub async fn get_latest_posts(page: i64, current_user: String) -> Result<Vec<BlogPost>, sqlite::Error> {

	let connection = sqlite::open("./grislygrotto.db")?;

	let mut stmt = connection.prepare(sql::SELECT_LATEST_POSTS)?;
	stmt.bind::<&[(_, Value)]>(&[
		(1, current_user.into()), 
		(2, 5.into()), 
		(3, (page * 5).into())])?;

	let mut final_result = Vec::new();

	let mut markdown_options = comrak::ComrakOptions::default();
	markdown_options.render.unsafe_ = true;

	while let Ok(State::Row) = stmt.next() {
		let post = mapping::post_from_statement(&stmt, &markdown_options)?;
		final_result.push(post);
	}

	Ok(final_result)
}
