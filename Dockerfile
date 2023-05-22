FROM rust:latest as builder
RUN apt-get update && apt-get -y install ca-certificates cmake libssl-dev && rm -rf /var/lib/apt/lists/*
RUN rustup default stable && rustup update

# cache crate compilation
RUN mkdir src
RUN echo "fn main() {}" > ./src/main.rs
COPY ["Cargo.toml", "Cargo.lock",  "./"]
RUN cargo build --release
COPY src src
RUN cargo build --release

FROM debian:latest
RUN apt-get update && apt-get -y install sqlite3 libsqlite3-dev ca-certificates
COPY --from=builder /target/release/grislygrotto .
EXPOSE 3000
CMD ["/grislygrotto","/mnt/db/grislygrotto.db"]