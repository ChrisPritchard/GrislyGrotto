FROM rust:latest as builder
RUN apt-get update && apt-get -y install ca-certificates cmake libssl-dev && rm -rf /var/lib/apt/lists/*
RUN rustup default stable && rustup update

COPY . .
RUN cargo build --release

FROM debian:latest
RUN apt-get update && apt-get -y install sqlite3 libsqlite3-dev ca-certificates
COPY --from=builder /target/release/grislygrotto .
EXPOSE 3000
CMD ["/grislygrotto", "/mnt/db/grislygrotto.db"]