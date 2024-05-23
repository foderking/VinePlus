#!/usr/bin/sh
cat $1.json |  jq -r '(.[] | [.Id,(.Thread | tojson),(.Board | tojson),.IsPinned,.IsLocked,.IsDeleted,.Type,.LastPostNo,.LastPostPage,.Created,.TotalPosts,.TotalView,(.Creator|tojson)]) | @csv' > $1.csv
