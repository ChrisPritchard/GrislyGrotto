use super::prelude::*;

#[get("/archives")]
async fn archives_page(tmpl: Data<Tera>) -> impl Responder {
    let context = tera::Context::new();
    let html = tmpl.render("archives", &context).expect("template rendering failed");
    HttpResponse::Ok().body(html)
}