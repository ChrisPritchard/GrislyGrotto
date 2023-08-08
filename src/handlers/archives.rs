use time::OffsetDateTime;

use crate::data;

use super::{prelude::*, *};

#[get("/archives")]
async fn archives_page(tmpl: Data<Tera>, session: Session) -> WebResponse {
    let current_user = session.get("current_user")?.unwrap_or(Some("".to_string())).unwrap();

    let counts = data::archives::get_month_counts(&current_user).await?;
    let stories = data::archives::get_stories(&current_user).await?;

    let mut total_posts = 0;
    for year in &counts {
        for month in &year.months {
            total_posts += month.count;
        }
    }
    let total_years = &counts[0].year.parse::<i64>().unwrap() - &counts[counts.len()-1].year.parse::<i64>().unwrap();

    let mut context = super::default_tera_context(&session)?;
    context.insert("years", &counts);
    context.insert("stories", &stories);
    context.insert("total_posts", &total_posts);
    context.insert("total_years", &total_years);

    let html = tmpl.render("archives", &context)?;
    ok(html)
}

#[derive(Deserialize)]
struct MonthQuery {
    month: String,
    year: String,
}

#[get("/archives/{month}/{year}")]
async fn posts_for_month(tmpl: Data<Tera>, path: Path<MonthQuery>, session: Session) -> WebResponse {
    let current_user = session.get("current_user")?.unwrap_or(Some("".to_string())).unwrap();
    let posts = data::archives::get_posts_in_month(&path.year, &path.month, &current_user).await?;

    if posts.len() == 0 {
        return Err(WebError::NotFound)
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

    let mut context = super::default_tera_context(&session)?;
    context.insert("posts", &posts);
    context.insert("year", &path.year);
    context.insert("month", &month);

    if !(&month == "June" && &path.year == "2006") {
        context.insert("prev_month", &prev_month);
        context.insert("prev_year", &prev_year);
    }
    let now = OffsetDateTime::now_local().unwrap();
    let now_month = now.month().to_string();
    let now_year = now.year().to_string();
    if !(&month == &now_month && &path.year == &now_year) {
        context.insert("next_month", &next_month);
        context.insert("next_year", &next_year);
    }

    let html = tmpl.render("month", &context)?;
    ok(html)
}