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
    pub word_count: i64,
    pub comment_count: i64,
}

// pub struct BlogComment {
//     pub id: i64,
//     pub author: String,
//     pub content: String,
//     pub date: String,
//     pub owned: bool
// }

// pub struct Author {
//     pub username: String,
//     pub password: String,
//     pub displayname: String,
//     pub imageurl: String,
// }

// pub struct YearSet {
//     pub year: String,
//     pub yonths: Vec<MonthCount>,
// }

// pub struct MonthCount {
//     pub month: String,
//     pub year: String,
//     pub count: i32
// }