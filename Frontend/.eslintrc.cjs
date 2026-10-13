module.exports = {
  root: true,
  env: { browser: true, es2020: true },
  extends: [
    'eslint:recommended',
    'plugin:@typescript-eslint/recommended',
    'plugin:react-hooks/recommended',
    'eslint-config-prettier',
    'prettier',
  ],
  ignorePatterns: ['dist', '.eslintrc.cjs'],
  parser: '@typescript-eslint/parser',
  plugins: ['react-refresh', 'import', 'react', '@typescript-eslint', 'prettier'],
  rules: {
    'react-refresh/only-export-components': ['warn', { allowConstantExport: true }],
    '@typescript-eslint/no-explicit-any': 'off',
    'prefer-const': 'off',
    'array-callback-return': 'off',
    'react-hooks/exhaustive-deps': 'off',
    '@typescript-eslint/naming-convention': 'off',
    '@typescript-eslint/no-explicit-any': 'off',
    'prettier/prettier': [
      'warn',
      {
        jsxSingleQuote: true,
        singleQuote: true,
        semi: true,
        tabWidth: 2,
        printWidth: 100,
        trailingComma: 'es5',
      },
    ],
    'prettier/prettier': [
      'error',
      {
        endOfLine: 'auto',
      },
    ],
    'no-restricted-imports': [
      'error',
      {
        patterns: ['.*'],
      },
    ],
    'import/no-unresolved': 'off',
    'import/named': 'off',
    'import/no-cycle': 'off',
    'import/no-extraneous-dependencies': 'off',
    'import/prefer-default-export': 'off', // Named export is easier to refactor automatically
    'import/extensions': 'off',
    'import/order': [
      'error',
      {
        groups: ['builtin', 'external', 'internal', ['parent', 'sibling', 'index'], 'object'],
        pathGroups: [
          {
            pattern:
              '{commons,components,constants,helpers,hooks,languages,libs,pages,redux,services,types,utils}/**/*',
            group: 'internal',
            position: 'before',
          },
        ],
        pathGroupsExcludedImportTypes: ['builtin'],
        'newlines-between': 'always',
        alphabetize: {
          order: 'asc',
          // "caseInsensitive": true
        },
      },
    ],
    'import/no-unused-modules': [
      'off', // only set "error" when testing
      {
        unusedExports: true,
        ignoreExports: ['src/pages', 'src/languages'],
      },
    ],
    'max-classes-per-file': 'off',
    'no-console': 'warn',
  },
};
