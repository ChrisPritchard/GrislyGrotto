use super::prelude::*;

#[get("/search")]
async fn search_page(tmpl: Data<Tera>) -> impl Responder {
    let context = tera::Context::new();
    let html = tmpl.render("search", &context).expect("template rendering failed");
    HttpResponse::Ok().body(html)
}