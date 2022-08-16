docker build -t nacos2demo:naming -f .\docker\dockerfile.naming .

docker run --rm --name nacos2naming -p 9877:9877 nacos2demo:naming