use crate::data;

use super::{prelude::*, *};

#[derive(Deserialize)]
struct SearchInfo {
    search_term: Option<String>,
}

#[get("/search")]
async fn search_page(tmpl: Data<Tera>, query: Query<SearchInfo>, session: Session) -> WebResponse {
    let mut context = super::default_tera_context(&session);

    if let Some(search_term) = &query.search_term {
        context.insert("search_term", &search_term);

        let results = data::search::get_search_results(&search_term, "aquinas").await?;

        if results.len() == 0 {
            context.insert("zero_results", &true);
        } else {
            context.insert("results", &results);
        }
    }

    let html = tmpl.render("search", &context).expect("template rendering failed");
    ok(html)
}