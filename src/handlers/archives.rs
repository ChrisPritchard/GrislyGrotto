use crate::data;

use super::prelude::*;

#[get("/archives")]
async fn archives_page(tmpl: Data<Tera>) -> impl Responder {
    
    let counts = data::archives::get_month_counts("aquinas".to_string()).await;
    if let Err(err) = counts {
        error!("error getting month counts: {}", err);
        return HttpResponse::InternalServerError().body("something went wrong")
    } 
    let counts = counts.unwrap();

    let stories = data::archives::get_stories("aquinas".to_string()).await;
    if let Err(err) = stories {
        error!("error getting stories: {}", err);
        return HttpResponse::InternalServerError().body("something went wrong")
    } 
    let stories = stories.unwrap();

    let mut total_posts = 0;
    for year in &counts {
        for month in &year.months {
            total_posts += month.count;
        }
    }
    let total_years = &counts[0].year.parse::<i64>().unwrap() - &counts[counts.len()-1].year.parse::<i64>().unwrap();

    let mut context = tera::Context::new();
    context.insert("years", &counts);
    context.insert("stories", &stories);
    context.insert("total_posts", &total_posts);
    context.insert("total_years", &total_years);

    let html = tmpl.render("archives", &context).expect("template rendering failed");
    HttpResponse::Ok().body(html)
}