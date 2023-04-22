use tera::Tera;

const MASTER_PAGE: &str = include_str!("master.html");
const PAGE_LATEST: &str = include_str!("latest.html");
const PAGE_SINGLE: &str = include_str!("single.html");

const PARSE_ERROR: &str = "template parsing failed";

pub fn template_engine() -> Tera {
    let mut tera = Tera::default();

    tera.add_raw_template("master", MASTER_PAGE).expect(PARSE_ERROR);

    tera.add_raw_template("latest", PAGE_LATEST).expect(PARSE_ERROR);
    tera.add_raw_template("single", PAGE_SINGLE).expect(PARSE_ERROR);

    tera
}