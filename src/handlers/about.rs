use super::prelude::*;

#[get("/about")]
async fn about_page(tmpl: Data<Tera>) -> impl Responder {
    let context = tera::Context::new();
    let html = tmpl.render("about", &context).expect("template rendering failed");
    HttpResponse::Ok().body(html)
}