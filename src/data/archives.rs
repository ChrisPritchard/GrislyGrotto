use crate::model::{YearSet, BlogPost};

use super::{prelude::*, *};

pub async fn get_month_counts(current_user: &str) -> Result<Vec<YearSet>> {
    let connection = db()?;

    let mut stmt = connection.prepare(sql::SELECT_MONTH_COUNTS)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, current_user.into()),])?;

    let mut all_years = Vec::new();
    let mut current_year: Option<YearSet> = None;

    while stmt.next()? == State::Row {
        let month = mapping::month_count_from_statement(&stmt)?;
        match current_year {
            None => {
                current_year = Some(YearSet { year: month.year.clone(), months: vec![month] })
            },
            Some(year) if year.year != month.year => {
                all_years.push(year);
                current_year = Some(YearSet { year: month.year.clone(), months: vec![month] })
            },
            Some(mut year) => {
                year.months.push(month);
                current_year = Some(year)
            }
        }        
    }

    all_years.push(current_year.unwrap());

    Ok(all_years)
}

pub async fn get_stories(current_user: &str) -> Result<Vec<BlogPost>> {
    let connection = db()?;

    let mut stmt = connection.prepare(sql::SELECT_STORIES)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, current_user.into()),])?;

    let mut final_result = Vec::new();
    let markdown_options = markdown_options();	

    while stmt.next()? == State::Row {
        let post = mapping::post_from_statement(&stmt, &markdown_options)?;
        final_result.push(post);
    }

    Ok(final_result)
}

pub async fn get_posts_in_month(year: &str, month: &str, current_user: &str) -> Result<Vec<BlogPost>> {

    let month_filter = format!("{year}-{}", mapping::index_of_month(&month));

    let connection = db()?;
    let mut stmt = connection.prepare(sql::SELECT_MONTH_POSTS)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, month_filter.into()), 
        (2, current_user.into()),])?;

    let mut final_result = Vec::new();
    let markdown_options = markdown_options();	

    while stmt.next()? == State::Row {
        let post = mapping::post_from_statement(&stmt, &markdown_options)?;
        final_result.push(post);
    }

    Ok(final_result)
}