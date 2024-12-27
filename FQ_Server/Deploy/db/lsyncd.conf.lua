settings {
	logfile = "/var/log/lsyncd/lsyncd.log",
	statusFile = "/var/log/lsyncd/lsyncd.status"
}
sync {
	default.rsync,
	source = "/var/lib/docker/volumes/FQDB.BackRestRepo/_data/",
	target = "/root/fq-drivehq/"
}
