use actix_web::{get, Responder, web::Path, HttpResponse};

static STATIC_CONTENT: &[(&str, (&str, &[u8]))] = &[
    ("site.css", ("text/css", include_bytes!("site.css").as_slice())),
    ("site-dark.css", ("text/css", include_bytes!("site-dark.css").as_slice())),
    ("style.js", ("text/javascript", include_bytes!("style.js").as_slice())),
    ("comments.js", ("text/javascript", include_bytes!("comments.js").as_slice())),
    ("login.js", ("text/javascript", include_bytes!("login.js").as_slice())),
    ("account.js", ("text/javascript", include_bytes!("account.js").as_slice())),
    ("editor.js", ("text/javascript", include_bytes!("editor.js").as_slice())),
];

#[get("/static/{file_path}")]
async fn static_content(file_path: Path<String>) -> impl Responder {
    let file_path = file_path.into_inner();
    let found = STATIC_CONTENT.iter().find(|(f, _)| *f == file_path).map(|(_, d)| *d);
    match found {
        None => HttpResponse::NotFound().body("file not found"),
        Some((ct, bytes)) => HttpResponse::Ok().content_type(ct).body(bytes).into()
    }
}