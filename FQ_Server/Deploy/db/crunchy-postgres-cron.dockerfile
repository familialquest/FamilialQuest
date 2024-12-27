FROM crunchydata/crunchy-postgres:centos7-12.3-4.3.2
USER root

RUN /bin/bash -c 'yum-config-manager --disable crunchypg12; \
yum install cronie -y; \
crontab_txt_path="/backrestrepo/crontab.txt"; \
cronjob="@reboot if [ -e $crontab_txt_path ]; then crontab $crontab_txt_path; fi;"; \
( crontab -l | grep -v -F "$croncmd" ; echo "$cronjob" ) | crontab -; \
if [ ! -z $FQ_CRON_CMDS ]; then echo "$FQ_CRON_CMDS" > temp_cron.txt; fi; \
cronjob="@reboot if [ ! -z $FQ_CRON_CMDS ]; then echo "$FQ_CRON_CMDS" > temp_cron.txt; crontab temp_cron.txt; fi;"; \
( crontab -l | grep -v -F "$croncmd" ; echo "$cronjob" ) | crontab -;'
	
USER 26