use super::prelude::*;

#[derive(Deserialize)]
struct SearchInfo {
    search_term: Option<String>,
}

#[get("/search")]
async fn search_page(tmpl: Data<Tera>, query: Query<SearchInfo>) -> impl Responder {
    let mut context = tera::Context::new();
    if query.search_term.is_some() {
        context.insert("search_term", &query.search_term);
    }

    context.insert("zero_results", &true);

    let html = tmpl.render("search", &context).expect("template rendering failed");
    HttpResponse::Ok().body(html)
}