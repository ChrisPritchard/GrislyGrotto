use crate::data;

use super::{prelude::*, *};

#[get("/login")]
async fn login_page(tmpl: Data<Tera>, session: Session) -> WebResponse {
    let context = super::default_tera_context(&session);
    let html = tmpl.render("login", &context).expect("template rendering failed");
    Ok(html)
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
        return Redirect("/".into())
    }

    let html = tmpl.render("login", &context).expect("template rendering failed");
    Ok(html)
}

#[get("/logout")]
async fn logout(session: Session) -> WebResponse {
    let _ = session.remove("current_user");
    Redirect("/".into())
}

#[get("/account")]
async fn account_details(tmpl: Data<Tera>, session: Session) -> WebResponse {
    let current_user: Option<String> = session.get("current_user").unwrap_or(None);
    if current_user.is_none() {
        return Err(WebError::NotFound)
    }
    let current_user = current_user.unwrap();
    
    let display_name = data::account::get_user_display_name(&current_user).await?.unwrap_or(current_user.clone());

    let mut context = super::default_tera_context(&session);
    context.insert("current_display_name", &display_name);
    context.insert("current_username", &current_user);

    let html = tmpl.render("account", &context).expect("template rendering failed");
    Ok(html)
}

#[derive(Deserialize)]
struct AccountForm {
    new_display_name: Option<String>,
    old_password: Option<String>,
    new_password: Option<String>,
    new_password_confirm: Option<String>,
}

#[post("/account")]
async fn update_account_details(tmpl: Data<Tera>, form: Form<AccountForm>, session: Session) -> WebResponse {
    let current_user: Option<String> = session.get("current_user").unwrap_or(None);
    if current_user.is_none() {
        return Err(WebError::NotFound)
    }
    let current_user = current_user.unwrap();
    
    if let Some(new_display_name) = &form.new_display_name {
        data::account::update_user_display_name(&current_user, &new_display_name).await?;
        return Redirect("/account".into())
    }

    if let Some(old_password) = &form.old_password {
        if let Some(new_password) = &form.new_password {
            if let Some(new_password_confirm) = &form.new_password_confirm {
                if new_password != new_password_confirm {
                    return Err(WebError::BadRequest("new password does not match".into()))
                }
                let valid = data::account::validate_user(&current_user, &old_password).await?;
                if !valid {
                    return Err(WebError::BadRequest("old password is not correct".into()))
                }

                data::account::update_user_password(&current_user, &new_password).await?;
                return Redirect("/account".into())
            }
        }
    }

    Err(WebError::BadRequest("invalid update".into()))
}