use actix_web::{HttpServer, App, web::Data};
use tera::Tera;

mod model;
mod data;
mod handlers;

const PAGE_LATEST: &str = include_str!("templates/latest.html");
const PAGE_SINGLE: &str = include_str!("templates/single.html");

#[actix_web::main]
async fn main() -> std::io::Result<()> {

    let mut tera = Tera::default();
    tera.add_raw_template("latest", PAGE_LATEST).expect("template parsing failed");
    tera.add_raw_template("single", PAGE_SINGLE).expect("template parsing failed");

    let server = HttpServer::new(move || {
        App::new()
            .app_data(Data::new(tera.clone()))
            .service(handlers::latest)
            .service(handlers::single)
            .service(handlers::static_content)
    });  

    server.bind("0.0.0.0:3000")?.run().await
}
