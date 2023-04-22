use super::prelude::*;

static STATIC_CONTENT: &[(&str, (&str, &[u8]))] = &[
    ("site.css", ("text/css", include_bytes!("../static/site.css").as_slice())),
    ("site.js", ("text/javascript", include_bytes!("../static/site.js").as_slice())),
];

#[get("/static/{file_path}")]
async fn static_content(file_path: web::Path<String>) -> impl Responder {
    let file_path = file_path.into_inner();
    let found = STATIC_CONTENT.iter().find(|(f, _)| *f == file_path).map(|(_, d)| *d);
    match found {
        None => HttpResponse::NotFound().body("file not found"),
        Some((ct, bytes)) => HttpResponse::Ok().content_type(ct).body(bytes).into()
    }
}