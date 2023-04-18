use actix_web::{Responder, HttpServer, App, get, web::{self, Data}, HttpResponse};
use sqlx::{SqlitePool, query};
use tera::Tera;

const PAGE_INDEX: &str = include_str!("templates/index.html");

static STATIC_CONTENT: &[(&str, (&str, &[u8]))] = &[
    ("site.css", ("text/css", include_bytes!("static/site.css").as_slice())),
    ("site.js", ("text/javascript", include_bytes!("static/site.js").as_slice())),
];

#[get("/")]
async fn index(db: Data<SqlitePool>, tmpl: Data<Tera>) -> impl Responder {
    
    let mut pool = db.acquire().await.unwrap();
    let result = query!("
        SELECT COUNT(Key) as count FROM Posts
        ").fetch_one(&mut pool).await.unwrap();

    let message = format!("Hello! There are {} posts in total.", result.count);
    
    let mut context = tera::Context::new();
    context.insert("welcome", &message);

    let html = tmpl.render("index", &context).expect("template rendering failed");
    HttpResponse::Ok().body(html)
}

#[get("/static/{file_path}")]
async fn static_content(file_path: web::Path<String>) -> impl Responder {
    let file_path = file_path.into_inner();
    let found = STATIC_CONTENT.iter().find(|(f, _)| *f == file_path).map(|(_, d)| *d);
    match found {
        None => HttpResponse::NotFound().body("file not found"),
        Some((ct, bytes)) => HttpResponse::Ok().content_type(ct).body(bytes).into()
    }
}

#[actix_web::main]
async fn main() -> std::io::Result<()> {

    let mut tera = Tera::default();
    tera.add_raw_template("index", PAGE_INDEX).expect("template parsing failed");

    let db = SqlitePool::connect("grislygrotto.db").await.expect("failed to access db");

    let server = HttpServer::new(move || {
        App::new()
            .app_data(web::Data::new(tera.clone()))
            .app_data(web::Data::new(db.clone()))
            .service(index)
            .service(static_content)
    });  

    server.bind("0.0.0.0:3000")?.run().await
}