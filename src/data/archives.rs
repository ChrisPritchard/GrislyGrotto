use crate::model::YearSet;

use super::{prelude::*, *};

pub async fn get_month_counts(current_user: String) -> Result<Vec<YearSet>> {
    let connection = db()?;

    let mut stmt = connection.prepare(sql::SELECT_MONTH_COUNTS)?;
    stmt.bind::<&[(_, Value)]>(&[
        (1, current_user.into()),])?;

    let mut all_years = Vec::new();
    let mut current_year: Option<YearSet> = None;

    while let Ok(State::Row) = stmt.next() {
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