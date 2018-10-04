module.exports = function (grunt)
{
    var allScssFiles = [
        'Scripts/libs/material/core/style/variables.scss',
        'Content/Styles/_variables.scss',
        'Scripts/libs/material/core/style/core-theme.scss',
        'Scripts/libs/material/core/style/mixins.scss',
        'Scripts/libs/material/core/style/layout.scss',
        'Scripts/libs/material/core/style/structure.scss',
        'Scripts/libs/material/core/style/typography.scss',
        'Scripts/libs/material/core/services/layout/layout-attributes.scss',
        'Scripts/libs/material/core/services/layout/layout.scss',
        'Scripts/libs/material/components/**/*.scss',
        'Content/Styles/Material/**/*.scss',
        'Scripts/app/directives/**/*.scss',
        'Scripts/app/overrides/**/*.scss',
        'Scripts/app/partials/**/*.scss',
        'Scripts/app/views/**/*.scss',
        'Content/Styles/common.scss'
    ];
    var allScssTmpFile = 'Content/all-scss.tmp';

    grunt.initConfig({

        concat: {
            // concat all css files into a single tmp file.
            sass: {
                dest: allScssTmpFile,
                src: allScssFiles
            },
            //material: {
            //    files: {
            //        src: ['Scripts/libs/material/components/**/*.js', 'Scripts/libs/material/core/**/*.js'],
            //        dest: 'Scripts/libs/material/material.js'
            //    }
            //}
            scripts: {
                dest: 'Scripts/libs/material/material.js',
                src: ['Scripts/libs/material/components/**/*.js', 'Scripts/libs/material/core/**/*.js']
            }
        },
        sass: {
            options: {
                sourceMap: false
            },
            base: {
                src: allScssTmpFile,
                dest: 'Content/styles.css'
            }
        },

        watch: {
            sass: {
                files: allScssFiles,
                tasks: ['concat-sass', 'sass']
            }
//            scripts: {
//                files: ['Crypto/**/*.js'],
//                tasks: ['concat-scripts']
//            }
        }
    });

    grunt.loadNpmTasks('grunt-contrib-watch');
    grunt.loadNpmTasks('grunt-contrib-concat');
    grunt.loadNpmTasks('grunt-sass');

    grunt.registerTask('concat-sass', ['concat:sass', 'sass']);
    grunt.registerTask('concat-scripts', ['concat:scripts']);
    grunt.registerTask('default', ['concat', 'sass', 'watch']);
};