use actix_web::{HttpServer, App, web::Data};
use tera::Tera;

mod model;
mod data;
mod handlers;

const PAGE_INDEX: &str = include_str!("templates/index.html");

#[actix_web::main]
async fn main() -> std::io::Result<()> {

    let mut tera = Tera::default();
    tera.add_raw_template("index", PAGE_INDEX).expect("template parsing failed");

    let server = HttpServer::new(move || {
        App::new()
            .app_data(Data::new(tera.clone()))
            .service(handlers::latest)
            .service(handlers::static_content)
    });  

    server.bind("0.0.0.0:3000")?.run().await
}
