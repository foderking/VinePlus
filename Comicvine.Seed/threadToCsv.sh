#!/usr/bin/sh
cat threads.json |  jq -r '(.[] | [.Id,(.Thread | tojson),(.Board | tojson),.IsPinned,.IsLocked,.IsDeleted,.Type,.LastPostNo,.LastPostPage,.Created,.TotalPosts,.TotalView,(.Creator|tojson)]) | @csv' > rand.csv
