use regex::Regex;

use crate::model::BlogPost;

use super::{prelude::*, *};

pub async fn get_search_results(search_term: &str, current_user: &str) -> Result<Vec<BlogPost>> {
    let connection = db()?;

    let filter_search_term = format!("%{}%", search_term);

    let mut stmt = connection.prepare(sql::SELECT_SEARCH_RESULTS)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, filter_search_term.clone().into()),
        (2, filter_search_term.clone().into()),
        (3, current_user.into()),])?;

    let mut final_result = Vec::new();
    let markdown_options = markdown_options();	

    let remove_html = Regex::new("<[^>]*>").unwrap();

    while stmt.next()? == State::Row {
        let mut post = mapping::post_from_statement(&stmt, &markdown_options)?;

        let content = remove_html.replace_all(&post.content, "");
        let term_loc = content.to_lowercase().find(&search_term.to_lowercase());
        if let Some(loc) = term_loc {
            let start = 0.max(loc as i64 - 20);
            let end = (content.len() - 1).min(loc + 20);
            let prepend = if start == 0 { "" } else { "..." };
            let append = if end == (content.len() - 1) { "" } else { "..." };
            post.content = format!("{prepend}{}{append}", &content[start as usize..end]);
        } else {
            post.content = format!("{}...", &content[..40]);
        }

        final_result.push(post);
    }

    Ok(final_result)
}