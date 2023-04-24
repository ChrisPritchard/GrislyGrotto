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

#[derive(Deserialize)]
struct MonthQuery {
    month: String,
    year: String,
}

#[get("/archives/{month}/{year}")]
async fn posts_for_month(tmpl: Data<Tera>, path: Path<MonthQuery>) -> impl Responder {

    let posts = data::archives::get_posts_in_month(&path.year, &path.month, "aquinas".to_string()).await;
    if let Err(err) = posts {
        error!("error getting month's posts: {}", err);
        return HttpResponse::InternalServerError().body("something went wrong")
    } 
    let posts = posts.unwrap();

    if posts.len() == 0 {
        return HttpResponse::NotFound().body("page not found")
    }

    let mut month_chars: Vec<char> = path.month.to_lowercase().chars().collect();
    month_chars[0] = month_chars[0].to_ascii_uppercase();
    let month: String = month_chars.iter().collect(); // oh rust, never change

    let (prev_month, next_month) = data::prev_next_month(&month).unwrap();
    let (mut prev_year, mut next_year) = (path.year.clone(), path.year.clone());
    if prev_month == "December" {
        prev_year = (prev_year.parse::<i64>().unwrap() - 1).to_string();
    } else if next_month == "January" {
        next_year = (next_year.parse::<i64>().unwrap() + 1).to_string();
    }

    let mut context = tera::Context::new();
    context.insert("posts", &posts);
    context.insert("year", &path.year);
    context.insert("month", &month);

    if !(&month == "June" && &path.year == "2006") {
        context.insert("prev_month", &prev_month);
        context.insert("prev_year", &prev_year);
    }
    let now = chrono::offset::Local::now();
    let now_month = format!("{}", now.format("%B"));
    let now_year = format!("{}", now.format("%Y"));
    if !(&month == &now_month && &path.year == &now_year) {
        context.insert("next_month", &next_month);
        context.insert("next_year", &next_year);
    }

    let html = tmpl.render("month", &context).expect("template rendering failed");
    HttpResponse::Ok().body(html)
}