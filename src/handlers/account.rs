use crate::data;

use super::{prelude::*, *};

#[get("/login")]
async fn login_page(tmpl: Data<Tera>, session: Session) -> WebResponse {
    let context = super::default_tera_context(&session);
    let html = tmpl.render("login", &context).expect("template rendering failed");
    ok(html)
}

#[derive(Deserialize)]
struct LoginForm {
    username: String,
    password: String,
    category: String, // just used for honeypot
}

#[post("/login")]
async fn try_login(form: Form<LoginForm>, tmpl: Data<Tera>, session: Session) -> WebResponse {

    if form.category != "user" || form.username.len() == 0 || form.password.len() == 0 {
        return Err(WebError::BadRequest("invalid comment".into()))
    }

    let valid = data::account::validate_user(&form.username, &form.password).await?;

    let mut context = super::default_tera_context(&session);
    if !valid {
        context.insert("error", "invalid username and/or password");
    } else {
        let _ = session.insert("current_user", form.username.clone());
        return redirect("/".into())
    }

    let html = tmpl.render("login", &context).expect("template rendering failed");
    ok(html)
}

#[get("/logout")]
async fn logout(session: Session) -> WebResponse {
    let _ = session.remove("current_user");
    redirect("/".into())
}

#[get("/account")]
async fn account_details(tmpl: Data<Tera>, session: Session) -> WebResponse {
    let current_user: Option<String> = session.get("current_user").unwrap_or(None);
    if current_user.is_none() {
        return redirect("/login".into())
    }
    let current_user = current_user.unwrap();
    
    let display_name = data::account::get_user_display_name(&current_user).await?.unwrap_or(current_user.clone());

    let mut context = super::default_tera_context(&session);
    context.insert("current_display_name", &display_name);
    context.insert("current_username", &current_user);

    let html = tmpl.render("account", &context).expect("template rendering failed");
    ok(html)
}

#[derive(Deserialize)]
struct UpdateDisplayNameForm {
    new_display_name: String,
}

#[post("/account/display_name")]
async fn update_user_display_name(form: Form<UpdateDisplayNameForm>, session: Session) -> WebResponse {
    let current_user: Option<String> = session.get("current_user").unwrap_or(None);
    if current_user.is_none() {
        return Err(WebError::Forbidden)
    }
    let current_user = current_user.unwrap();

    if form.new_display_name.len() < 1 {
        return Err(WebError::BadRequest("invalid display name".into()))
    }

    data::account::update_user_display_name(&current_user, &form.new_display_name).await?;
    redirect("/account?message=display+name+updated+successfully".into())
}

#[derive(Deserialize)]
struct UpdatePasswordForm {
    old_password: String,
    new_password: String,
    new_password_confirm: String,
}

#[post("/account/password")]
async fn update_user_password(form: Form<UpdatePasswordForm>, session: Session) -> WebResponse {
    let current_user: Option<String> = session.get("current_user").unwrap_or(None);
    if current_user.is_none() {
        return Err(WebError::Forbidden)
    }
    let current_user = current_user.unwrap();
    
    if form.new_password != form.new_password_confirm {
        return Err(WebError::BadRequest("new password does not match".into()))
    }
    
    if form.new_password.len() < 14 {
        return Err(WebError::BadRequest("new password must be a minimum of 14 characters".into()))
    }
    
    let valid = data::account::validate_user(&current_user, &form.old_password).await?;
    if !valid {
        return Err(WebError::BadRequest("old password is not correct".into()))
    }
    
    data::account::update_user_password(&current_user, &form.new_password).await?;
    redirect("/account?message=password+updated+successfully".into())
}
