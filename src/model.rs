use serde::Serialize;

#[derive(Clone, Debug, Serialize)]
pub struct BlogPost {
    pub author: String,
    pub author_username: String,
    pub key: String,
    pub title: String,
    pub content: String,
    pub date: String,
    pub is_story: bool,
    pub is_draft: bool,
    pub word_count: i64,
    pub comment_count: i64,
    pub comments: Option<Vec<BlogComment>>
}

#[derive(Clone, Debug, Serialize)]
pub struct BlogComment {
    pub id: i64,
    pub author: String,
    pub content: String,
    pub date: String,
    pub owned: bool
}

#[derive(Clone, Debug, Serialize)]
pub struct Author {
    pub username: String,
    pub password: String,
    pub displayname: String,
    pub imageurl: String,
}

#[derive(Clone, Debug, Serialize)]
pub struct YearSet {
    pub year: String,
    pub months: Vec<MonthCount>,
}

#[derive(Clone, Debug, Serialize)]
pub struct MonthCount {
    pub month: String,
    pub year: String,
    pub count: i64
}