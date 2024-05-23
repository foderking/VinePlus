#!/usr/bin/sh

cat $1.json | \
    jq -rn --stream 'fromstream(1 | truncate_stream(inputs)) | select(.) | [.Id,.IsComment,.IsDeleted,.IsModComment,.PostNo,(.Creator|tojson),.IsEdited,.Created,.Content,.ThreadId] | @csv' > $1.csv
