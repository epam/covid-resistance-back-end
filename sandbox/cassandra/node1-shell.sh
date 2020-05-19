#!/bin/bash

COMMAND=${1-bash}

docker exec -it cassandra_node1_1 ${COMMAND}
