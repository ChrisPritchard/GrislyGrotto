use tera::Tera;

const PARSE_ERROR: &str = "template parsing failed";

pub fn template_engine() -> Tera {
    let mut tera = Tera::default();

    tera.add_raw_template("master", include_str!("master.html")).expect(PARSE_ERROR);
    tera.add_raw_template("latest", include_str!("latest.html")).expect(PARSE_ERROR);
    tera.add_raw_template("single", include_str!("single.html")).expect(PARSE_ERROR);
    tera.add_raw_template("archives", include_str!("archives.html")).expect(PARSE_ERROR);
    tera.add_raw_template("month", include_str!("month.html")).expect(PARSE_ERROR);
    tera.add_raw_template("search", include_str!("search.html")).expect(PARSE_ERROR);
    tera.add_raw_template("about", include_str!("about.html")).expect(PARSE_ERROR);
    tera.add_raw_template("login", include_str!("login.html")).expect(PARSE_ERROR);
    tera.add_raw_template("account", include_str!("account.html")).expect(PARSE_ERROR);
    tera.add_raw_template("editor", include_str!("editor.html")).expect(PARSE_ERROR);

    tera
}