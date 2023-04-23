use tera::Tera;

const MASTER_PAGE: &str = include_str!("master.html");
const PAGE_LATEST: &str = include_str!("latest.html");
const PAGE_SINGLE: &str = include_str!("single.html");
const PAGE_ARCHIVES: &str = include_str!("archives.html");
const PAGE_SEARCH: &str = include_str!("search.html");
const PAGE_ABOUT: &str = include_str!("about.html");
const PARSE_ERROR: &str = "template parsing failed";

pub fn template_engine() -> Tera {
    let mut tera = Tera::default();

    tera.add_raw_template("master", MASTER_PAGE).expect(PARSE_ERROR);
    tera.add_raw_template("latest", PAGE_LATEST).expect(PARSE_ERROR);
    tera.add_raw_template("single", PAGE_SINGLE).expect(PARSE_ERROR);
    tera.add_raw_template("archives", PAGE_ARCHIVES).expect(PARSE_ERROR);
    tera.add_raw_template("search", PAGE_SEARCH).expect(PARSE_ERROR);
    tera.add_raw_template("about", PAGE_ABOUT).expect(PARSE_ERROR);

    tera
}